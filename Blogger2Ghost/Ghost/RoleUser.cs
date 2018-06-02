using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blogger2Ghost.Ghost
{
    public class RoleUser : IEquatable<RoleUser>
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }

        public bool Equals(RoleUser other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return UserId == other.UserId && RoleId == other.RoleId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RoleUser) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (UserId.GetHashCode() * 397) ^ RoleId.GetHashCode();
            }
        }

        public static bool operator ==(RoleUser left, RoleUser right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RoleUser left, RoleUser right)
        {
            return !Equals(left, right);
        }
    }
}
