using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.Formatting;

namespace TailBlazer.Views.Tail
{
    public class LineProxyFactory : ILineProxyFactory
    {
        private readonly ITextFormatter _textFormatter;
        private readonly ILineMatches _lineMatches;

        public LineProxyFactory(ITextFormatter textFormatter, ILineMatches lineMatches)
        {
            _textFormatter = textFormatter;
            _lineMatches = lineMatches;
        }

        public LineProxy Create(Line line)
        {
            return new LineProxy(line, _textFormatter.GetFormatter(line.Text), _lineMatches.GetMatches(line.Text));
        }
    }
}