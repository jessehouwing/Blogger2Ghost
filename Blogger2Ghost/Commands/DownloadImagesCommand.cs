using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Blogger2Ghost.Mapping;
using HtmlAgilityPack;
using ReverseMarkdown.Converters;

namespace Blogger2Ghost.Commands
{
    public class DownloadImagesCommand : BaseCommand
    {
        public DownloadImagesCommand()
        {
            IsCommand("images", "Download images and generate mapping file.");
            HasOption("include-drafts", "Include draft posts", _ => IncludeDrafts = true);
        }

        public bool IncludeDrafts
        {
            get { return _includeDrafts; }
            set { _includeDrafts = value; }
        }

        private readonly HashSet<string> _downloadedFiles = new HashSet<string>();

        public override int Run(string[] remainingArguments)
        {
            int result = 0;

            try
            {
                result &= base.Run(remainingArguments);
                var images = Posts.SelectMany(post => ExtractImages(post.Description));

                string imagesPath = Path.Combine(Out, "images");
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }

                List<ImageMapping> imageMapping = new List<ImageMapping>(images.Count());
                images.ToList().ForEach(img =>
                {
                    using (WebClient web = new WebClient())
                    {
                        Uri downloadUrl = img.First();
                        string target = ToTargetPath(downloadUrl, imagesPath);

                        try
                        {
                            web.DownloadFile(downloadUrl, target);
                            var relativePath = ToRelativePath(Out, target);
                            imageMapping.Add(new ImageMapping(img.Select(i => i.AbsoluteUri).ToArray(), relativePath));
                            Console.WriteLine("Downloaded : " + downloadUrl.AbsoluteUri);
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine("Failed : " + downloadUrl.AbsoluteUri);
                            Console.Error.WriteLine("Error : " + ex.Message);
                            imageMapping.Add(new ImageMapping(new []{ downloadUrl.AbsoluteUri }, downloadUrl.AbsoluteUri));
                        }
                    }
                });

                WriteFile("images", imageMapping.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                result = -1;
            }

            return result;
        }

        private string ToTargetPath(Uri img, string imagesPath)
        {
            var target = WebUtility.UrlDecode(Path.GetFileName(img.AbsolutePath));
            target = WebUtility.UrlDecode(target);
            target = Regex.Replace(target, "[^a-z0-9.]+", "-", RegexOptions.IgnoreCase);
            target = Path.Combine(imagesPath, target);
            target = Path.GetFullPath(target);
            if (!Path.HasExtension(target))
            {
                target += ".png";
            }

            if (_downloadedFiles.Contains(target))
            {
                string filename = Path.GetFileNameWithoutExtension(target);
                string extension = Path.GetExtension(target);
                string path = Path.GetDirectoryName(target);

                target = Path.Combine(path, $"{filename}_1{extension}");
            }

            _downloadedFiles.Add(target);
            return target;
        }

        private string ToRelativePath(string basePath, string target)
        {
            return new Uri(basePath, UriKind.Absolute)
                .MakeRelativeUri(new Uri(target, UriKind.Absolute)).ToString();
        }

        public IEnumerable<Uri[]> ExtractImages(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                yield break;
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var images =  doc.DocumentNode.Descendants("img").Where(img => img.Attributes["src"] != null);

            foreach (var image in images)
            {
                var imgUrl = new Uri(image.Attributes["src"].Value, UriKind.RelativeOrAbsolute);

                if (image.ParentNode.Name == "a" && image.ParentNode.Attributes["href"] != null)
                {
                    var aHref = new Uri(image.ParentNode.Attributes["href"].Value, UriKind.RelativeOrAbsolute);
                    if (Path.GetFileName(imgUrl.AbsolutePath) == Path.GetFileName(aHref.AbsolutePath))
                    {
                        yield return new[] {aHref, imgUrl};
                        
                        continue;
                    }
                }

                yield return new []{imgUrl};
            }
        }
    }
}
