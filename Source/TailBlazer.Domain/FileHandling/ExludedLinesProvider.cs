using System;
using System.Collections.Generic;
using System.Linq;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling;

public class ExludedLinesProvider : ILineProvider
{
    private readonly ILineProvider _lines;
    private readonly Func<string, bool> _itemsToExclude;

    public ExludedLinesProvider([NotNull] ILineProvider lines, Func<string, bool> itemsToExclude)
    {
        if (lines == null) throw new ArgumentNullException(nameof(lines));
        _lines = lines;
        _itemsToExclude = itemsToExclude;

        Count = lines.Count;
    }

    public int Count { get; }


    public IEnumerable<Line> ReadLines(ScrollRequest scroll)
    {
        return ReadLinesImpl(scroll).OrderBy(line => line.Index);

    }
    public IEnumerable<Line> ReadLinesImpl(ScrollRequest scroll)
    {
        var currentPage = _lines.ReadLines(scroll).Where(line => !_itemsToExclude(line.Text)).ToArray();
        foreach (var line in currentPage)
        {
            yield return line;
        }

        if (currentPage.Length == scroll.PageSize)
            yield break;

        var deficit = scroll.PageSize - currentPage.Length;

        //work backwards through the file until we have enough lines
        var traverseUpTail = YieldTail(scroll.PageSize, scroll.FirstIndex - scroll.PageSize, deficit);
        foreach (var line in traverseUpTail)
        {
            yield return line;
        }
    }

    private IEnumerable<Line> YieldTail(int pageSize, int firstIndex, int deficit)
    {
        if (firstIndex < 0)
            pageSize = pageSize + firstIndex;

        //ensure first index is non-zero
        firstIndex = Math.Max(0, firstIndex);

        return _lines.ReadLines(new ScrollRequest(ScrollReason.User, pageSize, firstIndex))
            .Where(line => !_itemsToExclude(line.Text))
            .Reverse()
            .Take(deficit);
    }
}