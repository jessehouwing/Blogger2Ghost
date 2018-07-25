using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Blogger2Ghost.Ghost;
using Blogger2Ghost.Mapping;
using HtmlAgilityPack;
using Microsoft.SyndicationFeed;
using ReverseMarkdown;
using ReverseMarkdown.Converters;

namespace Blogger2Ghost.Commands
{
    public class ConvertCommand : BaseCommand
    {
        public ConvertCommand()
        {
            IsCommand("convert", "Convert blogger export to ghost import");
            HasOption("include-drafts", "Include draft posts", _ => IncludeDrafts = true);
            HasOption("m|markdown", "Convert to Markdown", _ => Markdown = true);
            HasOption("template=", "Use this custom template", t => Template = t);
            HasOption("z|zip", "Generate zip file", _ => Zip = true);
            HasOption("redirect-permanent", "", _ => RedirectPermanent = true);
        }

        public bool RedirectPermanent { get; set; }

        public bool Zip { get; set; }

        public string Template { get; set; }

        public bool Markdown { get; set; }

        public bool IncludeDrafts
        {
            get { return _includeDrafts; }
            set { _includeDrafts = value; }
        }

        private UrlMapping[] _urlMappings;
        private ImageMapping[] _imageMappings;
        private UserMapping[] _userMappings;
        private TagMapping[] _tagMappings;

        public override int Run(string[] remainingArguments)
        {
            int result = 0;

            try
            {
                result &= base.Run(remainingArguments);

                _urlMappings = ReadFile<UrlMapping>("urls");
                _imageMappings = ReadFile<ImageMapping>("images");
                _userMappings = ReadFile<UserMapping>("authors");
                _tagMappings = ReadFile<TagMapping>("tags");

                Export export = new Export();
                DbRecord record = new DbRecord
                {
                    Meta = new Meta(),
                    Data = new Data()
                };
                ConvertTags(record.Data.Tags);
                ConvertUsers(record.Data.Users);
                ConvertPosts(record.Data.Posts);
                AssignTags(record.Data.PostsTags);
                export.Db.Add(record);

                WriteFile("ghost", export);

                if (Zip)
                {
                    PackageFiles();
                }

                GenerateRedirects();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                result = -1;
            }

            return result;
        }

        private void GenerateRedirects()
        {
            var redirects = _urlMappings.Where(url => url.FromUrl != url.ToUrl)
                .Select(url => 
                    new Redirect
                    {
                        From = "^" + new Uri(url.FromUrl).AbsolutePath + "$",
                        To = "/" + url.ToUrl + "/",
                        Permanent = RedirectPermanent
                    }
                ).ToArray();

            var tagRedirect = _tagMappings.SelectMany(GetRedirectForTagMappingRecursive).Distinct().ToArray();

            var rssRedirect = new Redirect
            {
                From = "^/feeds/posts/default$",
                To = "/rss/",
                Permanent = false
            };

            var dateRedirect = new Redirect
            {
                From = @"^/\d{4}/?(\d{2}/?)?$",
                To = "/",
                Permanent = false
            };

            WriteFile("redirects", redirects.Concat(tagRedirect).Concat(new []{rssRedirect, dateRedirect}));
        }

        private IEnumerable<Redirect> GetRedirectForTagMappingRecursive(TagMapping tag)
        {
            foreach (string bloggerTag in tag.BloggerTag)
            {
                yield return new Redirect
                {
                    From = "^/search/label/" + Uri.EscapeDataString(bloggerTag) + "$",
                    To = string.IsNullOrWhiteSpace(tag.Slug) ? "/" : "/tag/" + tag.Slug
                };

                //TODO: Instead of generating two redirects, generate case insensitive regex matcher for `bloggerTag`.
                string lowercaseBloggerTag = bloggerTag.ToLowerInvariant();
                if (!bloggerTag.Equals(lowercaseBloggerTag, StringComparison.InvariantCulture))
                {
                    yield return new Redirect
                    {
                        From = "^/search/label/" + Uri.EscapeDataString(lowercaseBloggerTag) + "$",
                        To = string.IsNullOrWhiteSpace(tag.Slug) ? "/" : "/tag/" + tag.Slug
                    };
                }


                if (tag.ChildTags != null && tag.ChildTags.Any())
                {
                    foreach (var childTag in tag.ChildTags)
                    {
                        foreach (var redirect in GetRedirectForTagMappingRecursive(childTag))
                        {
                            yield return redirect;
                        }
                    }
                }
            }
        }

        private void PackageFiles()
        {
            string outputFile = Path.Combine(Out, "ghost.zip");
            if (File.Exists(outputFile) && Overwrite)
            {
                File.Delete(outputFile);
            }

            using (ZipArchive zip = ZipFile.Open(outputFile, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(Path.Combine(Out, "ghost.json"), "ghost.json");

                foreach (var file in Directory.GetFiles(Path.Combine(Out, "images")))
                {
                    string filename = Path.GetFileName(file);
                    zip.CreateEntryFromFile(file, @"images/" + filename);
                }
            }
            Console.WriteLine("Written : " + outputFile);
        }

        private void AssignTags(IList<PostTag> postTags)
        {
            foreach (var sourcePost in Posts)
            {
                foreach (var tag in sourcePost.Categories
                    .Where(entry => entry.Scheme.Contains("ns#")).Select(tag => tag.Name))
                {
                    var originalUrl = (sourcePost.Links.SingleOrDefault(alternate => string.Equals(alternate.RelationshipType, "alternate"))
                                       ?? sourcePost.Links.SingleOrDefault(self => IncludeDrafts && string.Equals(self.RelationshipType, "self"))
                        ).Uri.AbsoluteUri;

                    if (_tagLookup.ContainsKey(tag))
                    {
                        if (!postTags.Any(pt => 
                            pt.PostId == _postLookup[originalUrl] 
                            && pt.TagId == _tagLookup[tag]))
                        {
                            var postTag = new PostTag()
                            {
                                PostId = _postLookup[originalUrl],
                                TagId = _tagLookup[tag],
                                SortOrder = GetSortOrderForTag(tag) ?? 0
                            };
                            postTags.Add(postTag);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Warning : Dropping tag: " + tag + " from post.");
                    }
                }
            }
        }

        private int? GetSortOrderForTag(string bloggerTag, IEnumerable<TagMapping> tagMap = null)
        {
            if (tagMap == null)
            {
                return GetSortOrderForTag(bloggerTag, _tagMappings);
            }

            int? result = null;
            
            foreach (var map in tagMap)
            {
                if (map.BloggerTag.Any(bt => bt == bloggerTag))
                {
                    return map.Order;
                }

                if (map.ChildTags?.Count > 0)
                {
                    result = GetSortOrderForTag(bloggerTag, map.ChildTags);
                }

                if (result != null)
                {
                    break;
                }
            }

            return result;
        }

        private readonly Dictionary<string, string> _postLookup = new Dictionary<string, string>();

        private void ConvertPosts(IList<Post> posts)
        {
            foreach (var sourcePost in Posts)
            {
                var isPublished = sourcePost.Links.Any(alternate => string.Equals(alternate.RelationshipType, "alternate"));
                var originalUrl = (sourcePost.Links.SingleOrDefault(alternate => string.Equals(alternate.RelationshipType, "alternate"))
                       ?? sourcePost.Links.SingleOrDefault(
                           self => IncludeDrafts
                           && string.Equals(self.RelationshipType, "self"))
                       ).Uri.AbsoluteUri;
                var slug = _urlMappings.Where(m => string.Equals(m.FromUrl, originalUrl)).Select(m => m.ToUrl).Single();
                Console.WriteLine("Converting page : " +slug);

                var author = _userLookup[sourcePost.Contributors.First().Uri];
                var html = ReplaceUrls(sourcePost.Description);
                var id = Guid.NewGuid().ToString();

                string markdown = ToMarkdown2(html);

                var targetPost = new Post
                {
                    Id = id,
                    AuthorId = author,
                    CreatedBy = author,
                    PublishedBy = author,
                    UpdatedBy = author,
                    Html = Markdown ? null : html,
                    Markdown = Markdown ? markdown : null,
                    Title = string.IsNullOrWhiteSpace(sourcePost.Title) ? "empty" : sourcePost.Title,
                    Slug = isPublished ? slug : "draft-" + id,
                    CreatedAt = sourcePost.Published.UtcDateTime,
                    PublishedAt = sourcePost.Published.UtcDateTime,
                    UpdatedAt = sourcePost.LastUpdated.UtcDateTime,
                    Page = false,
                    Status = isPublished ? PostStatus.Published : PostStatus.Draft,
                    CustomTemplate = "custom-" + Template,
                    CodeinjectionHead = $"<script>var bloggerUrl=\"{originalUrl}\";</script>"
                };

                posts.Add(targetPost);
                _postLookup.Add(originalUrl, targetPost.Id);
            }

            
        }

        private string ToMarkdown2(string html)
        {
            string markdown = html;
            if (Markdown && !string.IsNullOrWhiteSpace(html))
            {
                try
                {
                    Config config = new Config(githubFlavored: true);
                    Converter toMd = new Converter(config);
                    toMd.Unregister("pre");
                    toMd.Register("pre", new CodeConvertor());
                    markdown = toMd.Convert(html);
                }
                catch
                {
                    Console.Error.WriteLine("Eror : Markdown conversion failed.");
                }
            }

            return markdown;
        }

        private string ReplaceUrls(string sourcePostDescription)
        {
            if (string.IsNullOrWhiteSpace(sourcePostDescription))
            {
                return string.Empty;
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(sourcePostDescription);

            foreach (var imgTag in doc.DocumentNode.Descendants("img").Where(img => img.Attributes["src"] != null).ToArray())
            {
                string originalUrl = new Uri(imgTag.Attributes["src"].Value, UriKind.RelativeOrAbsolute).AbsoluteUri;
                ImageMapping newUrl = _imageMappings.FirstOrDefault(map => map.FromUrl.Any(u => u == originalUrl));

                imgTag.Attributes["src"].Value = newUrl?.ToPath ?? originalUrl;

                if (imgTag.ParentNode.Name == "a" && imgTag.ParentNode.Attributes["href"] != null)
                {
                    var href = Path.GetFileName(
                        new Uri(imgTag.ParentNode.Attributes["href"].Value, UriKind.RelativeOrAbsolute).AbsolutePath);
                    var src  = Path.GetFileName(new Uri(originalUrl, UriKind.RelativeOrAbsolute).AbsolutePath);

                    if (href == src)
                    {
                        imgTag.ParentNode.ParentNode.ReplaceChild(imgTag, imgTag.ParentNode);
                    }
                }
            }

            foreach (var aTag in doc.DocumentNode.Descendants("a").Where(img => img.Attributes["href"] != null)
                .ToArray())
            {
                try
                {
                    string originalUrl = new Uri(aTag.Attributes["href"].Value, UriKind.RelativeOrAbsolute).AbsoluteUri;
                    UrlMapping newUrl = _urlMappings.FirstOrDefault(map => map.FromUrl == originalUrl);

                    if (newUrl != null)
                    {
                        aTag.Attributes["href"].Value = "/" + newUrl.ToUrl;
                    }
                }
                catch (UriFormatException)
                {
                    Console.WriteLine("Invalid Url : " + aTag.Attributes["href"].Value);
                }
                
            }

            return doc.DocumentNode.OuterHtml;
        }

        private void ConvertUsers(IList<User> users)
        {
            foreach (var user in Authors)
            {
                ConvertUser(users, _userMappings, user);
            }
        }

        private readonly Dictionary<string, string> _userLookup = new Dictionary<string, string>();
            
        private void ConvertUser(IList<User> users, UserMapping[] mappings, ISyndicationPerson user)
        {
            var map = mappings.Single(candidate => string.Equals(candidate.FromGooglePlusUrl, user.Uri));
            var result = new User
            {
                Id = map?.Id ?? Guid.NewGuid().ToString(),
                Name = map?.Name ?? user.Name,
                Email = map.ToEmail,
                Slug = map.Slug
            };
            result.CreatedBy = result.Id;
            result.UpdatedBy = result.Id;

            _userLookup.Add(map.FromGooglePlusUrl, result.Id);
            users.Add(result);
        }

        private readonly Dictionary<string, string> _tagLookup = new Dictionary<string, string>();

        private void ConvertTags(IList<Tag> tags)
        {
            foreach (var mapping in _tagMappings)
            {
                ConvertTags(tags, mapping, null);
            }
        }

        private void ConvertTags(IList<Tag> tags, TagMapping mapping, Tag parent)
        {
            if (!string.IsNullOrWhiteSpace(mapping.Slug))
            {
                Tag tag = new Tag
                {
                    Slug = mapping.Slug,
                    Description = mapping.Description,
                    Name = mapping.Name ?? mapping.Slug,
                    Id = Guid.NewGuid().ToString(),
                    ParentId = parent?.Id
                };

                _tagLookup.Add(mapping.Slug, tag.Id);

                if (mapping.Aliases != null)
                {
                    foreach (var alias in mapping.Aliases)
                    {
                        _tagLookup.Add(alias, tag.Id);
                    }
                }

                tags.Add(tag);

                if (mapping.ChildTags != null)
                {
                    foreach (var child in mapping.ChildTags)
                    {
                        ConvertTags(tags, child, tag);
                    }
                }
            }
        }
    }

    internal class CodeConvertor : IConverter
    {
        public string Convert(HtmlNode node)
        {
            node.PrependChild(
                HtmlNode.CreateNode(Environment.NewLine + Environment.NewLine + "```" + Environment.NewLine));
            node.AppendChild(
                HtmlNode.CreateNode(Environment.NewLine + "```" + Environment.NewLine + Environment.NewLine));

            return string.Join(Environment.NewLine, node.Descendants("#text").Select(child => child.InnerText.Replace("&gt;", ">").Replace("&lt;", "<")));
        }
    }
}

