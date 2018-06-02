using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blogger2Ghost.Ghost
{
    public class Data
    {
        public Data()
        {
            Posts = new List<Post>();
            Tags = new List<Tag>();
            PostsTags = new List<PostTag>();
            Users = new List<User>();
        }

        public IList<Setting> Settings { get; }
        public IList<Post> Posts { get; }
        public IList<Tag> Tags { get; }
        public IList<PostTag> PostsTags { get; }
        public IList<User> Users { get; }
        public IList<RoleUser> RolesUsers { get; }
    }
}
