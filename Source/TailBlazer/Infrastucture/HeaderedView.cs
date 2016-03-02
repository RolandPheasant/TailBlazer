using System;

namespace TailBlazer.Infrastucture
{
    public class HeaderedView : IEquatable<HeaderedView>
    {
        public Guid Id { get; } = Guid.NewGuid();
        
        public HeaderedView(object header, object content)
        {
            Header = header;
            Content = content;
        }
        
        public object Header { get; }

        public object Content { get; }

        #region Equality

        public bool Equals(HeaderedView other)
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
            return Equals((HeaderedView) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(HeaderedView left, HeaderedView right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(HeaderedView left, HeaderedView right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}