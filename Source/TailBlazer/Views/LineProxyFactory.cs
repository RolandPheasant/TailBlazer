using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.Formatting;

namespace TailBlazer.Views
{
    public class LineProxyFactory : ILineProxyFactory
    {
        private readonly ITextFormatter _textFormatter;

        public LineProxyFactory(ITextFormatter textFormatter)
        {
            _textFormatter = textFormatter;
        }

        public LineProxy Create(Line line)
        {
            return new LineProxy(line, _textFormatter.GetFormatter(line.Text));
        }

    }
}