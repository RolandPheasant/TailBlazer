using System;
using TailBlazer.Domain.FileHandling;

namespace TailBlazer.Views
{
    public class LineProxy
    {
        private readonly Line _line;

        public LineProxy(Line line)
        {
            _line = line;
            IsRecent = line.Timestamp.HasValue && DateTime.Now.Subtract(line.Timestamp.Value).TotalSeconds < 2;
        }

        public int Number => _line.Number;
        public string Text => _line.Text;

        public bool IsRecent { get; }
    }
}