using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blogger2Ghost.Mapping
{
    public class UserMapping
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FromGooglePlusUrl { get; set; }
        public string ToEmail { get; set; }
        public string Slug { get; set; }
    }
}
