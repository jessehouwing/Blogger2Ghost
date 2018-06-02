using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SyndicationFeed;

namespace Blogger2Ghost.Mapping
{
    public class UrlMapping
    {

        public UrlMapping(string from, string to)
        {
            this.FromUrl = from;
            this.ToUrl = to;
        }

        public string FromUrl { get; set; }
        public string ToUrl { get; set; }
    }
}
