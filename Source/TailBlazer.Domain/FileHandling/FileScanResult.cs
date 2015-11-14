namespace TailBlazer.Domain.FileHandling
{
    public class FileScanResult
    {
        public FileNotification Notification { get;  }
        public LineIndex[] MatchingLines { get;  }
        public int TotalLines { get;  }
        public int EndOfTail { get;  }
        public int Index { get;  }

        public FileScanResult(FileNotification notification, LineIndex[] matchingLines, int totalLines, int endOfTail,int index)
        {
            Notification = notification;
            MatchingLines = matchingLines;
            TotalLines = totalLines;
            EndOfTail = endOfTail;
            Index = index;
        }
    }
}