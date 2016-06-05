using System;

namespace TailBlazer.Domain.StateHandling
{
    public struct StateBucketKey : IEquatable<StateBucketKey>
    {
        private readonly string _type;
        private readonly string _id;

        public StateBucketKey(string type, string id)
        {
            _type = type;
            _id = id;
        }

        #region Equality

        public bool Equals(StateBucketKey other)
        {
            return string.Equals(_type, other._type) && string.Equals(_id, other._id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is StateBucketKey && Equals((StateBucketKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_type?.GetHashCode() ?? 0)*397) ^ (_id?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(StateBucketKey left, StateBucketKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StateBucketKey left, StateBucketKey right)
        {
            return !left.Equals(right);
        }

        #endregion

        public override string ToString()
        {
            return $"{_type}, id={_id}";
        }
    }

}
