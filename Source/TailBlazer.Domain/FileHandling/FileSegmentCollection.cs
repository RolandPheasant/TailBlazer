using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TailBlazer.Domain.FileHandling
{
    public class FileSegmentCollection : IEquatable<FileSegmentCollection>
    {
        public FileInfo Info { get;  }
        public FileSegment[] Segments { get;  }
        public long TailStartsAt { get;  }
        public int Count { get;  }
        public FileSegmentChangedReason Reason { get; }
        public FileSegment Tail => Segments[Count - 1];
        public FileSegmentCollection Link { get; private set; }
        public long FileLength => Tail.End;
        public bool IsFirst { get; set; }

        public long FileSize { get; }


        public long SizeDiff { get; }

        public FileSegmentCollection(FileInfo fileInfo, FileSegment[] segments, long sizeDiff, bool isfirst)
        {
            if (segments.Length == 0)
                throw new ArgumentException("Argument is empty collection", nameof(segments));

            Info = fileInfo;
            Segments = segments;
            TailStartsAt = segments.Max(fs => fs.End);
            Count = Segments.Length;
            FileSize = TailStartsAt;
            SizeDiff = sizeDiff;
            Reason = FileSegmentChangedReason.Loaded;
            IsFirst = isfirst;
        }

        public FileSegmentCollection(FileSegmentCollection previous, FileInfo fileInfo, FileSegment[] segments,
            long sizeDiff, bool isfirst)
            : this(fileInfo, segments, sizeDiff, isfirst)
        {
            Link = previous;
        }

        public FileSegmentCollection(FileSegmentCollection fileSegmentCollection)
            : this(fileSegmentCollection.Info, fileSegmentCollection.Segments, fileSegmentCollection.SizeDiff, fileSegmentCollection.IsFirst)
        {
            Link = fileSegmentCollection.Link;
        }

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
            if (obj.GetType() != GetType()) return false;
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
    }
}