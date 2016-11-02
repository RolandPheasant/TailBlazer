using System;
using System.Linq;

namespace TailBlazer.Domain.FileHandling
{
    public sealed class FileSegmentCollection : IEquatable<FileSegmentCollection>
    {
        public IFileMetrics Metrics { get;  }
        public FileSegment[] Segments { get;  }
        public FileSegmentChangedReason Reason { get; }
        public FileSegment Tail { get; }
        public int Count => Segments.Length;
        public long FileSize => Metrics.Size;
        
        public FileSegmentCollection(IFileMetrics fileInfo, FileSegment[] segments)
        {
            Segments = segments.Length != 0 
                ? segments 
                : new[] { new FileSegment(FileSegmentType.Tail) };

            Tail = Segments.First(s => s.Type == FileSegmentType.Tail);
            Metrics = fileInfo;
            Reason = FileSegmentChangedReason.Loaded;
        }
        
        public FileSegmentCollection(IFileMetrics fileInfo, FileSegmentCollection previous)
        {
            Metrics = fileInfo;

            var previousTail = previous.Tail;
            var segments = previous.Segments;
            var tail = new FileSegment(previousTail, fileInfo.Size);
            segments[segments.Length-1] = tail;

            Tail = tail;
            Segments = segments;
            Reason = FileSegmentChangedReason.Tailed;
        }
        
        public FileSegmentCollection(IFileMetrics fileInfo)
        {
            var tail = new FileSegment(FileSegmentType.Tail);
            Segments = new[] { tail };
            Tail = tail;
            Metrics = fileInfo;
            Reason = FileSegmentChangedReason.New;
        }

        #region Equality

        public bool Equals(FileSegmentCollection other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Metrics, other.Metrics) 
                && Count == other.Count 
                && Reason == other.Reason 
                && FileSize == other.FileSize;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is FileSegmentCollection && Equals((FileSegmentCollection) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Metrics?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ Count;
                hashCode = (hashCode*397) ^ (int) Reason;
                hashCode = (hashCode*397) ^ FileSize.GetHashCode();
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
            return $"{nameof(Metrics)}: {Metrics}, {nameof(Count)}: {Count}, {nameof(FileSize)}: {FileSize}";
        }
    }
}