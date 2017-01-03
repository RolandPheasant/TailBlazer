using System;

namespace TailBlazer.Domain.FileHandling
{
    [Obsolete]
    public class LastTailInfo
    {

        public static readonly LastTailInfo None = new LastTailInfo();

        public long TailStartsAt { get;  }
        public DateTime LastTail { get;  }


        public LastTailInfo(long tailStartsAt)
        {
            TailStartsAt = tailStartsAt;
            LastTail = DateTime.UtcNow;
        }

        private LastTailInfo()
        {
        }
    }
}