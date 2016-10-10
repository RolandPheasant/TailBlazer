using System;
using System.Text;

namespace TailBlazer.Domain.FileHandling
{
    public class FileChanges : IEquatable<FileChanges>, IFileMetrics
    {
        private FileNotification Notification { get;  }

        public long SizeDiff { get; }

        public string FullName => Notification.FullName;

        public string Name => Notification.Name;

        public string Folder => Notification.Folder;

        public long Size => Notification.Size;

        public Encoding Encoding => Notification.Encoding;

        public bool NoChange { get; }

        public bool Invalidated { get; }

        public FileChanges(FileNotification fileNotification)
        {
            Notification = fileNotification;
            Invalidated = false;
            SizeDiff = Size;
            NoChange = false;
        }
        
        public FileChanges(FileChanges previous, FileNotification fileNotification)
        {
            Notification = fileNotification;
            SizeDiff = fileNotification.Size - previous.Size;
           
            Invalidated = previous.FullName != fileNotification.FullName || previous.Size > fileNotification.Size;
            NoChange = !Invalidated && previous.Size == fileNotification.Size;
        }

        #region Equality

        public bool Equals(FileChanges other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Size == other.Size && string.Equals(FullName, other.FullName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FileChanges)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Size.GetHashCode() * 397) ^ (FullName?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(FileChanges left, FileChanges right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FileChanges left, FileChanges right)
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