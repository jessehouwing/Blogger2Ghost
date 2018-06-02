using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blogger2Ghost.Ghost
{
    public class Export
    {
        public IList<DbRecord> Db { get; set; } = new List<DbRecord>();
    }

    public class DbRecord
    {
        public Meta Meta;
        public Data Data;
    }
}
