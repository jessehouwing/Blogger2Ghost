using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Blogger2Ghost.Ghost;
using ManyConsole;

namespace Blogger2Ghost
{
}

namespace Blogger2Ghost.Commands
{
    public class OptimizeRedirects : ConsoleCommand
    {
        private string _out;

        public OptimizeRedirects()
        {
            IsCommand("optimize-redirects", "Optimizes redirects generated with version 1.0");
            HasRequiredOption<string>("o|out=", "Target folder", o => Out = o);
            HasOption("f|force", "Overwrite existing files", _ => Overwrite = true);
        }

        public bool Overwrite
        {
            get;
            set;
        }

        public string Out
        {
            get { return _out; }
            set
            {
                if (!value.EndsWith("\\"))
                {
                    value += "\\";
                }

                _out = Path.GetDirectoryName(Path.GetFullPath(value)) + "\\";
            }
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var redirects = new List<Redirect>(FileHelper.ReadFile<Redirect>(Out, "redirects"));
                var optimizedRedirects = new List<Redirect>();

                foreach (Redirect r in redirects)
                {
                    var @from = RedirectHelper.NormalizeAndMakeCaseInsensitive(r.From);

                    if (!optimizedRedirects.Any(o => string.Equals(RedirectHelper.NormalizeFrom(o.From), RedirectHelper.NormalizeFrom(r.From), StringComparison.InvariantCultureIgnoreCase)))
                    {
                        optimizedRedirects.Add(r);
                    }
                }

                FileHelper.WriteFile(Out, "redirects", optimizedRedirects.ToArray(), Overwrite);
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }
        }
    }
}
