using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ManyConsole;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Atom;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Formatting = Newtonsoft.Json.Formatting;

namespace Blogger2Ghost.Commands
{
    public abstract class BaseCommand : ConsoleCommand
    {
        public string In { get; private set; }
        protected bool _includeDrafts = false;
        private string _out;
        private bool _overwrite;

        protected JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                DefaultContractResolver contractResolver = new DefaultContractResolver();
                contractResolver.NamingStrategy = new SnakeCaseNamingStrategy();
                var settings = new JsonSerializerSettings();
                settings.Formatting = Formatting.Indented;
                settings.ContractResolver = contractResolver;

                return settings;
            }
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

        public bool Overwrite => _overwrite;

        protected BaseCommand()
        {
            HasRequiredOption<string>("i|in=", "Blogger export file", f => In = f);
            HasRequiredOption<string>("o|out=", "Target folder", o => Out = o);
            HasOption("f|force", "Overwrite existing files", _ => _overwrite = true);
        }

        protected IQueryable Blogger { get; private set; }

        protected IQueryable<IAtomEntry> Posts
        {
            get
            {
                return Blogger?.OfType<IAtomEntry>()
                    .Where(entry => entry.Categories.Any(category => category.Name.Contains("#post")))
                    .Where(post => _includeDrafts || post.Links.Any(alternate => string.Equals(alternate.RelationshipType, "alternate")));
            }
        }

        protected IQueryable<string> Tags
        {
            get
            {
                return Posts?.SelectMany(post => post.Categories
                        .Where(entry => entry.Scheme.Contains("ns#")))
                    .Select(tag => tag.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase);
            }
        }

        protected IQueryable<ISyndicationPerson> Authors
        {
            get
            {
                return Blogger?.OfType<ISyndicationPerson>().Where(person =>
                    string.Equals(person.RelationshipType, "author", StringComparison.OrdinalIgnoreCase));
            }
        }

        protected IQueryable<IAtomEntry> Settings
        {
            get
            {
                return Posts?.OfType<IAtomEntry>()
                    .Where(entry => entry.Categories.Any(category => category.Name.Contains("#settings")));
            }
        }


        protected async Task<IQueryable> ReadBloggerData()
        {
            using (var file = File.OpenText(In))
            using (var xmlReader = XmlReader.Create(file))
            {
                var result = await ParseExport(xmlReader);
                return result.AsQueryable();
            }
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                Blogger = ReadBloggerData().Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return 0;
        }
        protected void WriteFile(string name, object data)
        {
            string path = Path.Combine(Out, name + ".json");
            Encoding utf8WithoutBom = new UTF8Encoding(false);
            using (var output = File.Open(path, Overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write))
            using (var writer = new StreamWriter(output, utf8WithoutBom))
            {
                writer.Write(JsonConvert.SerializeObject(data, JsonSerializerSettings));
                Console.WriteLine("Written : " + path);
            }
        }

        protected T[] ReadFile<T>(string name)
        {
            string path = Path.Combine(Out, name + ".json");
            using (var input = File.Open(path, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(input, Encoding.UTF8))
            {
                Console.WriteLine("Reading : " + path);
                return JsonConvert.DeserializeObject<T[]>(reader.ReadToEnd(), JsonSerializerSettings);
            }
        }

        private static async Task<List<object>> ParseExport(XmlReader xmlReader)
        {
            List<object> result = new List<object>();
            {
                var feedReader = new AtomFeedReader(xmlReader);
                while (await feedReader.Read())
                {
                    switch (feedReader.ElementType)
                    {
                        // Read category
                        case SyndicationElementType.Category:
                            ISyndicationCategory category = await feedReader.ReadCategory();
                            result.Add(category);
                            break;
                        // Read Image
                        case SyndicationElementType.Image:
                            ISyndicationImage image = await feedReader.ReadImage();
                            result.Add(image);
                            break;
                        // Read Item
                        case SyndicationElementType.Item:
                            ISyndicationItem item = await feedReader.ReadItem();
                            result.Add(item);
                            break;

                        // Read link
                        case SyndicationElementType.Link:
                            ISyndicationLink link = await feedReader.ReadLink();
                            result.Add(link);
                            break;

                        // Read Person
                        case SyndicationElementType.Person:
                            ISyndicationPerson person = await feedReader.ReadPerson();
                            result.Add(person);
                            break;

                        // Read content
                        default:
                            ISyndicationContent content = await feedReader.ReadContent();
                            result.Add(content);
                            break;
                    }
                }

                return result;
            }
        }
    }
}