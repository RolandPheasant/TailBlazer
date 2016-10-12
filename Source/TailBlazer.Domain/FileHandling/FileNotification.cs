using System;
using System.IO;
using System.Text;

namespace TailBlazer.Domain.FileHandling
{
    public sealed class FileNotification : IEquatable<FileNotification>, IFileMetrics
    {
        private FileInfo Info { get; }
        public bool Exists { get; }
        public long Size { get; }
        public string FullName => Info.FullName;
        public string Name => Info.Name;
        public string Folder => Info.DirectoryName;
        public FileNotificationReason Reason { get; }
        public Exception Error { get; }

        public Encoding Encoding { get; }

        public FileNotification(FileInfo fileInfo)
        {
            fileInfo.Refresh();
            Info = fileInfo;
            Exists = fileInfo.Exists;

            if (Exists)
            {
                Reason = FileNotificationReason.CreatedOrOpened;
                Size = Info.Length;
                Encoding = this.GetEncoding();
            }
            else
            {
                Reason = FileNotificationReason.Missing;
            }
        }

        public FileNotification(FileInfo fileInfo, Exception error)
        {
            Info = fileInfo;
            Error = error;
            Exists = false;
            Reason = FileNotificationReason.Error;
        }

        public FileNotification(FileNotification previous)
        {
            previous.Info.Refresh();

            Info = previous.Info;
            Exists = Info.Exists;

            if (Exists)
            {
                Size = Info.Length;

                Encoding = previous.Encoding ?? this.GetEncoding();

                if (!previous.Exists)
                {
                    Reason = FileNotificationReason.CreatedOrOpened;
                }
                else if (Size > previous.Size)
                {
                    Reason = FileNotificationReason.Changed;
                }
                else if (Size < previous.Size)
                {
                    //File has shrunk. We need it's own notification
                    Reason = FileNotificationReason.CreatedOrOpened;
                }

                else
                {
                    Reason = FileNotificationReason.None;
                }

            }
            else
            {
                Reason = FileNotificationReason.Missing;
            }
        }

        #region Equality

        public static explicit operator FileInfo(FileNotification source)
        {
            return source.Info;
        }

        public bool Equals(FileNotification other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(FullName, other.FullName) 
                   && Exists == other.Exists 
                   && Size == other.Size 
                   && Reason == other.Reason;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FileNotification) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = FullName?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ Exists.GetHashCode();
                hashCode = (hashCode*397) ^ Size.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Reason;
                return hashCode;
            }
        }

        public static bool operator ==(FileNotification left, FileNotification right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FileNotification left, FileNotification right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"{Name}  Size: {Size}, Type: {Reason}";
        }
    }
}