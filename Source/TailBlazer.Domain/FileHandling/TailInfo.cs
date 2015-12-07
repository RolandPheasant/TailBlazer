using System;

namespace TailBlazer.Domain.FileHandling
{
    public class TailInfo
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
    }
}