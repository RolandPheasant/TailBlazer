using System;
using TailBlazer.Domain.FileHandling;

namespace TailBlazer.Views
{
    public class LineProxy
    {
        public Line Line { get; }
        public long Number => Line.Number;
        public string Text => Line.Text;
        public bool IsRecent { get; }

        public LineProxy(Line line)
        {
            Line = line;
            IsRecent = line.Timestamp.HasValue && DateTime.Now.Subtract(line.Timestamp.Value).TotalSeconds < 2;
        }


  
    }
}