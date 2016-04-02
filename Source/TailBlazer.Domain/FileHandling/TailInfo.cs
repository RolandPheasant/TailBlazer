using System;

namespace TailBlazer.Domain.FileHandling
{
    public class TailInfo : IComparable
    {

        public static readonly TailInfo None = new TailInfo();

        public long TailStartsAt { get;  }
        public DateTime LastTail { get;  }


        public TailInfo(long tailStartsAt)
        {
            TailStartsAt = tailStartsAt;
            LastTail = DateTime.Now;
        }

        private TailInfo()
        {
        }

        public int CompareTo(object obj)
        {
            var other = obj as TailInfo;
            return LastTail.CompareTo(other?.LastTail);
        }
    }
}