using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.WebPages;
using HtmlAgilityPack;
using MarkdownSharp;

namespace NuGet.Docs
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            WebPageHttpHandler.RegisterExtension("md");
            WebPageHttpHandler.RegisterExtension("markdown");

            // Register the markdown virtual path factory
            VirtualPathFactoryManager.RegisterVirtualPathFactory(new MarkdownPathFactory());
        }

        public class MarkdownPathFactory : IVirtualPathFactory
        {
            public object CreateInstance(string virtualPath)
            {
                return new MarkdownWebPage();
            }

            public bool Exists(string virtualPath)
            {
                if (virtualPath.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                    || virtualPath.EndsWith(".markdown", StringComparison.OrdinalIgnoreCase))
                {
                    return HostingEnvironment.VirtualPathProvider.FileExists(virtualPath);
                }

                return false;
            }
        }

        /// <summary>
        /// Each markdown page is a web page that has this harcoded logic
        /// </summary>
        public class MarkdownWebPage : WebPage
        {
            // Set the cache timeout to 1 day (we'll also have cache dependencies)
            private const int CacheTimeout = 24 * 60 * 60;
            private const string OutlineLayout = "~/_Layout-Outline.cshtml";
            private static List<string> _virtualPathDependencies = new List<string>
        {
            "~/_PageStart.cshtml", 
            "~/_Layout.cshtml",
            OutlineLayout
        };

            public override void ExecutePageHierarchy()
            {
                this.Layout = OutlineLayout;
                base.ExecutePageHierarchy();
            }

            public override void Execute()
            {
                InitalizeCache();

                Page.Title = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Path.GetFileNameWithoutExtension(VirtualPath).Replace('-', ' ')).Replace("Nuget", "NuGet");
                Page.Source = GetSourcePath();
                Page.GeneratedDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss tt UTC");

                // Get the page content
                string markdownContent = GetMarkdownContent();

                // Transform the markdown
                string transformed = Transform(markdownContent);

                // Write the raw html it to the response (unencoded)
                WriteLiteral(transformed);
            }

            private void InitalizeCache()
            {
                _virtualPathDependencies.Add(VirtualPath);
                CacheDependency cd = HostingEnvironment.VirtualPathProvider.GetCacheDependency(VirtualPath, _virtualPathDependencies.ToArray(), DateTime.UtcNow);
                Response.AddCacheDependency(cd);
                Response.OutputCache(CacheTimeout);
            }

            /// <summary>
            /// Transforms the raw markdown content into html
            /// </summary>
            private string Transform(string content)
            {
                var githubMarkdown = new Octokit.MiscellaneousClient(new Octokit.Connection(new Octokit.ProductHeaderValue("NuGet.Docs")));
                string fileContents = null;

                try
                {
                    // Try to transform the content using GitHub's API
                    var request = githubMarkdown.RenderRawMarkdown(content);
                    request.Wait();

                    if (request.IsCompleted)
                    {
                        fileContents = request.Result;
                        Page.Generator = "GitHub";
                    }
                }
                catch
                {
                    // If the call to GitHub failed, then we'll swallow the exception
                    // and in the finally block, we'll use MarkdownSharp as a fallback.
                }
                finally
                {
                    if (fileContents == null)
                    {
                        fileContents = new Markdown().Transform(content);
                        Page.Generator = "MarkdownSharp";
                    }
                }

                return ProcessTableOfContents(fileContents);
            }

            /// <summary>
            /// Takes HTML and parses out all heading and sets IDs for each heading. Then sets the Headings property on the page.
            /// </summary>
            private string ProcessTableOfContents(string content)
            {
                var doc = new HtmlDocument();
                doc.OptionUseIdAttribute = true;
                doc.LoadHtml(content);

                var allNodes = doc.DocumentNode.Descendants();
                var allHeadingNodes = allNodes
                    .Where(node =>
                        node.Name.Length == 2
                        && node.Name.StartsWith("h", System.StringComparison.InvariantCultureIgnoreCase)
                        && Char.IsDigit(node.Name[1]));

                var headings = new List<Heading>();
                foreach (var heading in allHeadingNodes)
                {
                    string id = heading.InnerText.Replace(" ", "-").ToLowerInvariant(); ;

                    // GitHub gives us anchors in the headings, MarkdownSharp doesn't
                    HtmlNode anchor = heading.SelectSingleNode("a");

                    if (anchor != null)
                    {
                        // Note that the text of the heading is not within the anchor
                        // Get the name of the anchor as our id (but provide our existing id as the default)
                        id = anchor.GetAttributeValue("name", id);

                        // GitHub likes to prefix the names with: user-content- but we'll strip that off
                        if (id.StartsWith("user-content-"))
                        {
                            id = id.Substring(13);
                        }
                    }
                    else
                    {
                        // Create our anchor
                        anchor = HtmlAgilityPack.HtmlNode.CreateNode("<a></a>");
                        heading.ChildNodes.Insert(0, anchor);
                    }

                    // Skip the heading if the id ended up empty somehow (like an empty heading)
                    if (id != null)
                    {
                        anchor.SetAttributeValue("name", HttpUtility.HtmlAttributeEncode(id.ToLowerInvariant()));
                        headings.Add(new Heading(id, Convert.ToInt32(heading.Name[1]), heading.InnerText));
                    }
                }

                Page.Headings = headings;

                var docteredHTML = new StringWriter();
                doc.Save(docteredHTML);
                return docteredHTML.ToString();
            }

            /// <summary>
            /// Turns app relative urls (~/foo/bar) into the resolved version of that url for this page.
            /// </summary>
            private string ProcessUrls(string content)
            {
                return Regex.Replace(content, @"\((?<url>~/.*?)\)", match => "(" + Href(match.Groups["url"].Value) + ")");
            }

            /// <summary>
            /// Returns the markdown content within this page
            /// </summary>
            private string GetMarkdownContent()
            {
                VirtualFile file = HostingEnvironment.VirtualPathProvider.GetFile(VirtualPath);
                Stream stream = file.Open();
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }

            /// <summary>
            /// Returns the source file path for the virtual path on the request, with case sensitivity.
            /// </summary>
            /// <remarks>
            /// It's a shame nothing in the framework seems to do this for the path as a whole.  FileInfo
            /// and DirectoryInfo, among others, return the path using the casing specified.  And
            /// VirtualPathProvider.GetDirectory does not return the correct casing, but GetFile does.
            /// <para>
            /// So, we walk up the path and get the case sensitive name for each segment and then piece
            /// it all back together.
            /// </para>
            /// </remarks>
            private string GetSourcePath()
            {
                string requestFilePath = VirtualPath;
                Stack<string> pathSegments = new Stack<string>();

                do
                {
                    VirtualFile segment = HostingEnvironment.VirtualPathProvider.GetFile(requestFilePath);

                    if (segment != null && segment.Name != null)
                    {
                        pathSegments.Push(segment.Name);
                    }

                    int lastSlash = requestFilePath.LastIndexOf('/');
                    if (lastSlash > 0)
                    {
                        requestFilePath = requestFilePath.Substring(0, lastSlash);
                    }
                    else
                    {
                        break;
                    }
                }
                while (requestFilePath != null && requestFilePath.Length > 1 && requestFilePath.Contains('/'));

                return String.Join("/", pathSegments);
            }
        }
    }

    public class Heading
    {
        public string ID { get; private set; }
        public int Level { get; private set; }
        public string Text { get; private set; }

        public Heading(string id, int level, string text)
        {
            ID = id;
            Level = level;
            Text = text;
        }
    }

    public class Topic
    {
        private const string MetadataFile = "_metadata";

        public string Title { get; private set; }
        public string Url { get; private set; }
        public IEnumerable<Topic> SubTopics { get; private set; }

        public Topic()
        {
            SubTopics = new List<Topic>();
        }

        /// <summary>
        /// Gets a list of topics from the a directory. Only supports one level of nesting.
        /// </summary>
        public static IEnumerable<Topic> GetTopicsWithSubTopics(string virtualPath)
        {
            VirtualDirectory topicDir = HostingEnvironment.VirtualPathProvider.GetDirectory(virtualPath);
            Func<string, Metadata> getTopicMetadata = GetMetadataMapping(virtualPath);

            return from directory in topicDir.Directories.Cast<VirtualDirectory>()
                   let title = GetTitle(directory)
                   let metadata = getTopicMetadata(title)
                   let getSubTopicMetadata = GetMetadataMapping(directory.VirtualPath)
                   orderby metadata.Order, title
                   select new Topic
                   {
                       Title = title,
                       SubTopics = from file in directory.Files.Cast<VirtualFile>()
                                   let subTitle = GetTitle(file)
                                   let subMetadata = getSubTopicMetadata(subTitle)
                                   where !subTitle.Equals(MetadataFile, StringComparison.OrdinalIgnoreCase)
                                        && (Path.GetExtension(file.Name) == ".md" || Path.GetExtension(file.Name) == ".markdown")
                                   orderby subMetadata.Order, subTitle
                                   select new Topic
                                   {
                                       Title = subTitle,
                                       Url = GetUrl(file)
                                   }
                   };

        }

        public static IEnumerable<Topic> GetSubTopics(string virtualPath)
        {
            VirtualDirectory topicDir = HostingEnvironment.VirtualPathProvider.GetDirectory(virtualPath);
            Func<string, Metadata> getTopicMetadata = GetMetadataMapping(virtualPath);

            return from file in topicDir.Files.Cast<VirtualFile>()
                   let subTitle = GetTitle(file)
                   let subMetadata = getTopicMetadata(subTitle)
                   where !subTitle.Equals(MetadataFile, StringComparison.OrdinalIgnoreCase)
                        && (Path.GetExtension(file.Name) == ".md" || Path.GetExtension(file.Name) == ".markdown")
                   orderby subMetadata.Order, subTitle
                   select new Topic
                   {
                       Title = subTitle,
                       Url = GetUrl(file)
                   };
        }

        /// <summary>
        /// The order mapping is a file named order in the same virtual path.
        /// </summary>
        private static Func<string, Metadata> GetMetadataMapping(string virtualPath)
        {
            var vpp = HostingEnvironment.VirtualPathProvider;
            string metadataFile = VirtualPathUtility.AppendTrailingSlash(virtualPath) + MetadataFile;

            var mapping = new Dictionary<string, Metadata>();
            int index = 0;
            if (vpp.FileExists(metadataFile))
            {
                VirtualFile file = vpp.GetFile(metadataFile);
                Stream stream = file.Open();
                using (var reader = new StreamReader(stream))
                {
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        mapping[Normalize(line)] = new Metadata
                        {
                            Order = index++
                        };
                    }
                }
            }

            return title =>
            {
                Metadata metadata;
                if (mapping.TryGetValue(title, out metadata))
                {
                    return metadata;
                }
                return Metadata.Empty;
            };
        }

        private static string GetTitle(VirtualDirectory dir)
        {
            return Normalize(dir.VirtualPath.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries).Last());
        }

        private static string GetTitle(VirtualFile file)
        {
            return Normalize(Path.GetFileNameWithoutExtension(file.VirtualPath));
        }

        private static string Normalize(string path)
        {
            return path.Replace("-", " ").Trim();
        }

        private static string GetUrl(VirtualFile file)
        {
            string dir = VirtualPathUtility.GetDirectory(file.VirtualPath);
            string filePath = Path.GetFileNameWithoutExtension(file.VirtualPath);
            return VirtualPathUtility.Combine(dir, filePath).ToLowerInvariant();
        }

        private class Metadata
        {
            public static readonly Metadata Empty = new Metadata()
            {
                Order = Int32.MaxValue
            };

            public int Order { get; set; }
        }
    }
}