using System;
using System.Collections.Generic;
using DynamicData.Binding;
using TailBlazer.Domain.FileHandling;

namespace TailBlazer.Views
{
    public class LineProxy: IComparable<LineProxy>, IComparable
    {

        public static readonly IComparer<LineProxy> DefaultSort = SortExpressionComparer<LineProxy>
            .Ascending(p => p.Line.LineInfo.Start)
            .ThenByAscending(p => p.Line.LineInfo.Offset);

        public Line Line { get; }
        public long Number => Line.LineInfo.Start;

        public string Text => Line.Text;
        public bool IsRecent { get; }

        public LineProxy(Line line)
        {
            Line = line;
            IsRecent = line.Timestamp.HasValue && DateTime.Now.Subtract(line.Timestamp.Value).TotalSeconds < 1;
        }



        public int CompareTo(LineProxy other)
        {
            return DefaultSort.Compare(this, other);
        }

        public int CompareTo(object obj)
        {
            return CompareTo((LineProxy) obj);
        }
    }
}