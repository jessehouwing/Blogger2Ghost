using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Blogger2Ghost.Commands
{
    public class RedirectHelper
    {
        public static string NormalizeAndMakeCaseInsensitive(string @from)
        {
            @from = NormalizeFrom(@from);
            return MakeCaseInsensitive(@from);
        }

        public static string MakeCaseInsensitive(string @from)
        {
            @from = Regex.Replace(@from, @"[A-Z]", match =>
            {
                string upper = match.Value;
                return "[" + upper + upper.ToLowerInvariant() + "]";
            });
            return @from;
        }

        public static string NormalizeFrom(string @from)
        {
            @from = Uri.UnescapeDataString(@from);
            if (@from.StartsWith("^"))
            {
                @from = @from.Substring(1);
            }

            if (@from.EndsWith("$"))
            {
                @from = @from.Substring(0, @from.Length - 1);
            }

            @from = @from.Replace(@"\/", @"/");
            @from = @from.Replace(@"\.", @".");

            @from = Regex.Replace(@from, @"\[(?<chars>[a-zA-Z]+)\]", match =>
            {
                string chars = match.Groups["chars"].Value;
                if (chars.Length == 2
                    && chars[0].ToString().ToUpperInvariant()
                    == chars[1].ToString().ToUpperInvariant())
                {
                    return chars[0].ToString().ToUpperInvariant();
                }

                return match.Value;
            });

            @from = string.Join("/", @from.Split(new[] {@"/"}, StringSplitOptions.None).Select(part => Uri.EscapeDataString(part)));
            @from = "^" + @from + "$";
            return @from;
        }
    }
}