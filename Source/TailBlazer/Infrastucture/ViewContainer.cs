using System;

namespace TailBlazer.Infrastucture
{
    public interface ITitleProvider
    {
        string Title { get; }
    }

    public class ViewContainer : IEquatable<ViewContainer>
    {
        public Guid Id { get; } = Guid.NewGuid();

        
        public ViewContainer(object header, object content)
        {
            
            Header = header;
            Content = content;
        }

        public ViewContainer(ITitleProvider content)
        {

            Header = content.Title;
            Content = content;
        }

        public object Header { get; }

        public object Content { get; }

        #region Equality

        public bool Equals(ViewContainer other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ViewContainer) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(ViewContainer left, ViewContainer right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ViewContainer left, ViewContainer right)
        {
            return !Equals(left, right);
        }

        #endregion
        
    }
}