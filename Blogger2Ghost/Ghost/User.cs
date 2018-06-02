using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blogger2Ghost.Ghost
{
    public class User : IEquatable<User>
    {
        /*
        "id":           2,
        "name":         "user's name",
        "slug":         "users-name",
        "email":        "user@example.com",
        "image":        null,
        "cover":        null,
        "bio":          null,
        "website":      null,
        "location":     null,
        "accessibility": null,
        "status":       "active",
        "language":     "en_US",
        "meta_title":   null,
        "meta_description": null,
        "last_login":   null,
        "created_at":   1283780649000, // epoch time in millis
        "created_by":   1, // the first user created has an id of 1
        "updated_at":   1286958624000, // epoch time in millis
        "updated_by":   1 // the first user created has an id of 1
        */

        public string Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Email { get; set; }
        public string Image { get; set; }
        public string Cover { get; set; }
        public string Bio { get; set; }
        public string Website { get; set; }
        public string Location { get; set; }
        public string Accessibility { get; set; }
        public UserStatus Status { get; set; }
        public string Language { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }

        public bool Equals(User other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && string.Equals(Slug, other.Slug) && string.Equals(Email, other.Email);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((User) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ (Slug != null ? Slug.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Email != null ? Email.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(User left, User right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(User left, User right)
        {
            return !Equals(left, right);
        }
    }

    public enum UserStatus
    {
        Active
    }
}
