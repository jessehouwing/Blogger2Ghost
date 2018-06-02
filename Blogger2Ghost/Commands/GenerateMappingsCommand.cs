using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Blogger2Ghost.Mapping;
using ManyConsole;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Blogger2Ghost.Commands
{
    public class GenerateMappingsCommand : BaseCommand
    {
        public bool GenerateTags { get; set; }
        public bool GenerateAuthors { get; private set; }
        public bool GenerateUrls { get; private set; }

        public GenerateMappingsCommand()
        {
            IsCommand("mapping");
            HasOption("tags", "Generate Tag mapping", _ => GenerateTags = true);
            HasOption("authors", "Generate Author mapping", _ => GenerateAuthors = true);
            HasOption("urls", "Generate Url mapping", _ => GenerateUrls = true);
            HasOption("from=", "From Url (Regex)", from => FromUrl = from);
            HasOption("to=", "To Url (RegexReplace)", to => ToUrl = to);
            HasOption("include-drafts", "Include draft posts", _ => IncludeDrafts = true);
            HasOption("a|all", "Generate all mappings", _ =>
            {
                GenerateTags = true;
                GenerateAuthors = true;
                GenerateUrls = true;
            });
        }

        public bool IncludeDrafts
        {
            get { return _includeDrafts; }
            set { _includeDrafts = value; }
        }

        public string ToUrl { get; set; } = "$0";

        public string FromUrl { get; set; } = ".*";

        public override int Run(string[] remainingArguments)
        {
            int result = 0;

            try
            {
                result &= base.Run(remainingArguments);
                if (GenerateTags)
                {
                    WriteTags();
                }

                if (GenerateAuthors)
                {
                    WriteAuthors();
                }

                if (GenerateUrls)
                {
                    WriteUrls();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                result = -1;
            }

            return result;
        }

        private void WriteUrls()
        {
            Regex urlFix = new Regex(FromUrl, RegexOptions.Compiled);

            var urls = Posts.Select(post =>
                    post.Links.SingleOrDefault(alternate => string.Equals(alternate.RelationshipType, "alternate"))
                    ?? post.Links.SingleOrDefault(self => IncludeDrafts && string.Equals(self.RelationshipType, "self"))
                )
                .Select(link => link.Uri)
                .Select(url => new UrlMapping(url.AbsoluteUri, urlFix.Replace(url.AbsoluteUri, ToUrl)))
                .ToArray();
        
             WriteFile("urls", urls);
        }

        private void WriteAuthors()
        {
            var authors = base.Authors.Select(author => new UserMapping()
            {
                Name = author.Name,
                FromGooglePlusUrl = author.Uri,
                Slug = Regex.Replace(author.Name, "[^a-z0-9]+", "-", RegexOptions.IgnoreCase).ToLowerInvariant()
            }).ToArray();

            WriteFile("authors", authors);
        }

        private void WriteTags()
        {
            var tagMappings = Tags.OrderBy(tag => tag).Select(tag => new TagMapping(tag));

            WriteFile("tags", tagMappings);
        }
    }
}