using System;
using System.IO;
using System.Linq;
using System.Text;

namespace TailBlazer.Domain.FileHandling
{
    public sealed class FileSegmentCollection : IEquatable<FileSegmentCollection>
    {
        public IFileMetrics Metrics { get;  }
        public FileSegment[] Segments { get;  }
        public long TailStartsAt { get;  }
        public int Count { get;  }
        public FileSegmentChangedReason Reason { get; }
        public FileSegment Tail => Segments[Count - 1];
        public long FileLength => Tail.End;
        public long FileSize { get; }
        public long SizeDiff { get; }

        public FileSegmentCollection(IFileMetrics fileInfo, FileSegment[] segments, long sizeDiff)
        {
            if (segments.Length == 0)
                throw new ArgumentException("Argument is empty collection", nameof(segments));

            Metrics = fileInfo;
            Segments = segments;
            TailStartsAt = segments.Max(fs => fs.End);
            Count = Segments.Length;
            FileSize = TailStartsAt;
            SizeDiff = sizeDiff;
            Reason = FileSegmentChangedReason.Loaded;
        }

        public FileSegmentCollection(long newLength, FileSegmentCollection previous)
        {
            SizeDiff = newLength - previous.FileLength;

            //All this assumes it is the tail which has changed, but that may not be so
            Reason = FileSegmentChangedReason.Tailed;
            Metrics = previous.Metrics;

            var last = previous.Tail;
            TailStartsAt = last.End;

            var segments = previous.Segments;
            segments[segments.Length-1] = new FileSegment(last, newLength);
            Segments = segments;
            Count = Segments.Length;
            FileSize = newLength;

        }

        //private FileSegmentCollection()
        //{
        //}

        #region Equality

        public bool Equals(FileSegmentCollection other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Segments, other.Segments) && TailStartsAt == other.TailStartsAt && Count == other.Count && Reason == other.Reason;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FileSegmentCollection) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Segments?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ TailStartsAt.GetHashCode();
                hashCode = (hashCode*397) ^ Count;
                hashCode = (hashCode*397) ^ (int) Reason;
                return hashCode;
            }
        }

        public static bool operator ==(FileSegmentCollection left, FileSegmentCollection right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FileSegmentCollection left, FileSegmentCollection right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"{nameof(TailStartsAt)}: {TailStartsAt}, {nameof(Count)}: {Count}, {nameof(Reason)}: {Reason}, {nameof(FileSize)}: {FileSize}, {nameof(SizeDiff)}: {SizeDiff}";
        }
    }
}