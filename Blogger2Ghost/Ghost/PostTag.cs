using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blogger2Ghost.Ghost
{
    public class PostTag : IEquatable<PostTag>
    {
        public string TagId { get; set; }
        public string PostId { get; set; }
        public int SortOrder { get; set; }

        public bool Equals(PostTag other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TagId == other.TagId && PostId == other.PostId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PostTag) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (TagId.GetHashCode() * 397) ^ PostId.GetHashCode();
            }
        }

        public static bool operator ==(PostTag left, PostTag right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PostTag left, PostTag right)
        {
            return !Equals(left, right);
        }
    }
}
