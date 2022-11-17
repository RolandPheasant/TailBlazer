using System;
using System.Reactive.Linq;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.Formatting;
using TailBlazer.Infrastructure.Virtualisation;

namespace TailBlazer.Views.Tail;

public class LineProxyFactory : ILineProxyFactory
{
    private readonly ITextFormatter _textFormatter;
    private readonly ILineMatches _lineMatches;
    private readonly IThemeProvider _themeProvider;
    private readonly IObservable<TextScrollInfo> _textScroll;

    public LineProxyFactory([NotNull] ITextFormatter textFormatter, 
        [NotNull] ILineMatches lineMatches,
        [NotNull] IObservable<TextScrollInfo> textScrollObservable, 
        [NotNull] IThemeProvider themeProvider)
    {
        if (textScrollObservable == null) throw new ArgumentNullException(nameof(textScrollObservable));
        _textFormatter = textFormatter ?? throw new ArgumentNullException(nameof(textFormatter));
        _lineMatches = lineMatches ?? throw new ArgumentNullException(nameof(lineMatches));
        _themeProvider = themeProvider ?? throw new ArgumentNullException(nameof(themeProvider));
        _textScroll = textScrollObservable.StartWith(new TextScrollInfo(0,0));
    }
        
    public LineProxy Create(Line line)
    {
        return new LineProxy(line, _textFormatter.GetFormatter(line.Text), _lineMatches.GetMatches(line.Text), _textScroll, _themeProvider);
    }
}