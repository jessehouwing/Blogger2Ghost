using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blogger2Ghost.Ghost
{
    public class Redirect
    {
        /*
         {
  "from": "regex" ,
  "to":  "/url/",
  "permanent": true | false
}
         */

        public string From { get; set; }
        public string To { get; set; }
        public bool Permanent { get; set; }
    }
}
