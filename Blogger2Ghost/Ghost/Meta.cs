using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blogger2Ghost.Ghost
{
    public class Meta : IEquatable<Meta>
    {
        public DateTime Epoch
        {
            get;
            set;
        } = DateTime.UtcNow;

        public string Version { get; set; } = "003";


        public bool Equals(Meta other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Epoch == other.Epoch && string.Equals(Version, other.Version);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Meta) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Epoch.GetHashCode() * 397) ^ (Version != null ? Version.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Meta left, Meta right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Meta left, Meta right)
        {
            return !Equals(left, right);
        }
    }
}
