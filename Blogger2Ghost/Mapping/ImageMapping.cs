using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Blogger2Ghost.Mapping
{
    public class ImageMapping
    {
        public ImageMapping(string[] fromUrl, string toPath)
        {
            FromUrl = fromUrl;
            ToPath = toPath;
        }

        public string[] FromUrl { get; set; }
        public string ToPath { get; set; }
    }
}
