using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Blogger2Ghost.Ghost
{
    public class Post : IEquatable<Post>
    {
        /*
          "id":5,
                "title":        "my blog post title",
                "slug":         "my-blog-post-title",
                "markdown":     "the *markdown* formatted post body",
                "html":         "the <i>html</i> formatted post body",
                "image":        null,
                "featured":     0, // boolean indicating featured status
                "page":         0, // boolean indicating if this is a page or post
                "status":       "published", // or draft
                "language":     "en_US",
                "meta_title":   null,
                "meta_description":null,
                "author_id":    1, // the first user created has an id of 1
                "created_at":   1283780649000, // epoch time in millis
                "created_by":   1, // the first user created has an id of 1
                "updated_at":   1286958624000, // epoch time in millis
                "updated_by":   1, // the first user created has an id of 1
                "published_at": 1283780649000, // epoch time in millis
                "published_by": 1 // the first user created has an id of 1
         */

        public string Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Markdown { get; set; }
        public string Html { get; set; }
        public string Image { get; set; }
        public bool Featured { get; set; }
        public bool Page { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PostStatus Status { get; set; }
        public string Language { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string AuthorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime PublishedAt { get; set; }
        public string PublishedBy { get; set; }
        public string CodeinjectionHead { get; set; }
        public string CodeinjectionFoot { get; set; }
        public string CustomTemplate { get; set; }

        public bool Equals(Post other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && string.Equals(Slug, other.Slug);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Post) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id.GetHashCode() * 397) ^ (Slug?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(Post left, Post right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Post left, Post right)
        {
            return !Equals(left, right);
        }
    }

    public enum PostStatus
    {
        Published, Draft
    }
}
