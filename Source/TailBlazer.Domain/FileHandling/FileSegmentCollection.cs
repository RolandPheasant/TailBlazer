using System.IO;

namespace TailBlazer.Domain.FileHandling;

public sealed class FileSegmentCollection : IEquatable<FileSegmentCollection>
{
    public FileInfo Info { get;  }
    public FileSegment[] Segments { get;  }
    public long TailStartsAt { get;  }
    public int Count { get;  }
    public FileSegmentChangedReason Reason { get; }
    public FileSegment Tail => Segments[Count - 1];
    public long FileLength => Tail.End;

    public long FileSize { get; }


    public long SizeDiff { get; }

    public FileSegmentCollection(FileInfo fileInfo, FileSegment[] segments, long sizeDiff)
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
    }

    public FileSegmentCollection(long newLength, FileSegmentCollection previous)
    {
        SizeDiff = newLength - previous.FileLength;

        //All this assumes it is the tail which has changed, but that may not be so
        Reason = FileSegmentChangedReason.Tailed;
        Info = previous.Info;

        var last = previous.Tail;
        TailStartsAt = last.End;

        var segments = previous.Segments;
        segments[segments.Length-1] = new FileSegment(last, newLength);
        Segments = segments;
        Count = Segments.Length;
        FileSize = newLength;

    }
        
    #region Equality

    public bool Equals(FileSegmentCollection other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Equals(Segments, other.Segments) && TailStartsAt == other.TailStartsAt && Count == other.Count && Reason == other.Reason;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
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
}