using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Blogger2Ghost.Ghost
{
    public class Redirect : IEquatable<Redirect>
    {
        /*
         {
  "from": "regex" ,
  "to":  "/url/",
  "permanent": true | false
}
         */

        private string _from;
        public string From {
            get { return _from; }
            set
            {
                _from = value;
                _from = Regex.Replace(_from, @"(?<!\\)/", @"\/");
                _from = Regex.Replace(_from, @"(?<!\\)\.", @"\.");
            }
        }

        public string To { get; set; }
        public bool Permanent { get; set; }


        public bool Equals(Redirect other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(From, other.From) && string.Equals(To, other.To);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Redirect) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((From != null ? From.GetHashCode() : 0) * 397) ^ (To != null ? To.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Redirect left, Redirect right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Redirect left, Redirect right)
        {
            return !Equals(left, right);
        }
    }
}
