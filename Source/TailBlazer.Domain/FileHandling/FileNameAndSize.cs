using System;

namespace TailBlazer.Domain.FileHandling
{
    public class FileNameAndSize : IEquatable<FileNameAndSize>
    {
        public long Size { get; }
        public long SizeDiff { get; }
        public string Name { get; }

        public bool Invalidated { get; }

        public FileNameAndSize(FileNotification fileNotification)
        {
            Size = fileNotification.Size;
            Name = fileNotification.FullName;
            Invalidated = false;
            SizeDiff = Size;
        }


        public FileNameAndSize(FileNameAndSize previous, FileNotification fileNotification)
        {
            Size = fileNotification.Size;
            SizeDiff = fileNotification.Size - previous.Size;
            Name = fileNotification.FullName;

            Invalidated = previous.Name != fileNotification.FullName || previous.Size > fileNotification.Size;
        }

        #region Equality

        public bool Equals(FileNameAndSize other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Size == other.Size && string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FileNameAndSize)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Size.GetHashCode() * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

        public static bool operator ==(FileNameAndSize left, FileNameAndSize right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FileNameAndSize left, FileNameAndSize right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"{Name} ({Size.FormatWithAbbreviation()}), StartScanning = {Invalidated}";
        }
    }
}