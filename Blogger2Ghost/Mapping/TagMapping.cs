using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Blogger2Ghost.Mapping
{
    public class TagMapping
    {
        public TagMapping()
        {
        }

        public TagMapping(string bloggerTag)
        {
            Aliases = new List<string>();
            ChildTags = new List<TagMapping>();

            if (string.IsNullOrWhiteSpace(bloggerTag))
            {
                throw new ArgumentNullException(nameof(bloggerTag));
            }

            Slug = bloggerTag.ToLowerInvariant();
            Slug = Regex.Replace(Slug, "[^a-z0-9]+", "-", RegexOptions.IgnoreCase);

            if (!string.Equals(bloggerTag, Slug))
            {
                Aliases.Add(bloggerTag);
            }
        }

        public string Slug { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Order { get; set; } = 0;
        public IList<string> Aliases { get; set; }
        public IList<TagMapping> ChildTags { get; set; }
    }
}

