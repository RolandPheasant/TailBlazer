using System;
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

        public LineProxyFactory(ITextFormatter textFormatter, 
            ILineMatches lineMatches, 
            IObservable<TextScrollInfo> textScrollObservable=null)
        {
            _textFormatter = textFormatter;
            _lineMatches = lineMatches;
            _textScroll = textScrollObservable;
        }
        
        public LineProxy Create(Line line)
        {
            return new LineProxy(line, _textFormatter.GetFormatter(line.Text), _lineMatches.GetMatches(line.Text), _textScroll);
        }
    }
}