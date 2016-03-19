using System;
using System.Reactive.Linq;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.Formatting;
using TailBlazer.Infrastucture.Virtualisation;

namespace TailBlazer.Views.Tail
{
    public class LineProxyFactory : ILineProxyFactory
    {
        private readonly ITextFormatter _textFormatter;
        private readonly ILineMatches _lineMatches;
        private readonly IObservable<TextScrollInfo> _textScroll;

        public LineProxyFactory([NotNull] ITextFormatter textFormatter, 
            [NotNull] ILineMatches lineMatches,
            [NotNull] IObservable<TextScrollInfo> textScrollObservable)
        {
            if (textFormatter == null) throw new ArgumentNullException(nameof(textFormatter));
            if (lineMatches == null) throw new ArgumentNullException(nameof(lineMatches));
            if (textScrollObservable == null) throw new ArgumentNullException(nameof(textScrollObservable));

            _textFormatter = textFormatter;
            _lineMatches = lineMatches;
            _textScroll = textScrollObservable.StartWith(new TextScrollInfo(0,0));
        }
        
        public LineProxy Create(Line line)
        {
            return new LineProxy(line, _textFormatter.GetFormatter(line.Text), _lineMatches.GetMatches(line.Text), _textScroll);
        }
    }
}