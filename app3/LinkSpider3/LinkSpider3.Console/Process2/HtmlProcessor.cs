using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using HtmlAgilityPack;

using LinkSpider3.Process2.Extensions;

namespace LinkSpider3.Process2
{
    public class HtmlProcessor
    {
        public List<LinkInfo> Links { get; private set; }

        Stream stream;
        string baseUrl;
        TldParser tldParser;

        public HtmlProcessor(
            string baseUrl, Stream stream, TldParser tldParser)
        {
            this.stream = stream;
            this.baseUrl = baseUrl;
            this.tldParser = tldParser;

            Parse();
        }

        private void Parse()
        {
            if (this.stream.IsNull())
            {
                Links = new List<LinkInfo>();
                return;
            }


            HtmlDocument doc = new HtmlDocument();
            doc.Load(this.stream);

            string title = doc.DocumentNode.SelectSingleNode("//title").InnerText;
            HtmlNodeCollection anchors = doc.DocumentNode.SelectNodes("//a[@href]");

            if (anchors.IsNull())
            {
                Links = new List<LinkInfo>();
                return;
            }

            Links = anchors
                .Where(anchor => !anchor.Attributes["href"].Value.NormalizeUri(this.baseUrl).IsNullOrEmpty())
                .Select(
                    anchor => new LinkInfo(this.tldParser)
                    {
                        Href = anchor.Attributes["href"].Value.NormalizeUri(this.baseUrl),
                        AnchorKind = "anchor",
                        AnchorRel = anchor.GetAttributeValue("rel", "dofollow"),
                        AnchorText = (anchor.InnerText.IsNullOrEmpty() ? "<no text>" : anchor.InnerText),
                        Title = title
                    })
                .Distinct()
                .ToList();
        }


        public class LinkInfo
            : IEqualityComparer<LinkInfo>
        {
            public string AnchorText { get; set; }
            public string AnchorRel { get; set; }
            public string AnchorKind { get; set; }
            public string Domain { get; private set;  }
            public string DomainOrSubdomain { get; private set; }
            public string DomainScheme { get; private set; }
            public string Tld { get; private set; }
            public string Title { get; set; }
            public int Status { get; set; }

            private string href;
            public string Href 
            {
                get { return href; }
                set
                {
                    href = value;
                    
                    Uri uri = href.ToUri();
                    if (uri == null)
                        throw new ArgumentException("LinkInfo.Href is passed with an invalid url.");
                    
                    DomainScheme = uri.Scheme;
                    DomainOrSubdomain = uri.Host;
                    Tld = this.tldParser.GetTld(uri.Host);

                    // Get the correct domain
                    string hostNoTld = uri.Host.Remove(uri.Host.Length - (Tld.Length + 1));
                    int lastDot = hostNoTld.LastIndexOf('.');
                    Domain = hostNoTld.Substring(lastDot + 1) + "." + Tld;
                }
            }

            public bool Equals(LinkInfo x, LinkInfo y)
            {
                return
                    x.Href == y.Href &&
                    x.AnchorText == y.AnchorText &&
                    x.AnchorRel == y.AnchorRel &&
                    x.AnchorKind == y.AnchorKind;
            }

            public int GetHashCode(LinkInfo obj)
            {
                int hash =
                    Convert.ToInt32(
                        obj.Href.ToRabinFingerPrint() ^
                        obj.AnchorKind.ToRabinFingerPrint() ^
                        obj.AnchorRel.ToRabinFingerPrint() ^
                        obj.AnchorText.ToRabinFingerPrint());
                return hash.GetHashCode();
            }

            private TldParser tldParser;
            public LinkInfo(TldParser tldParser)
            {
                this.tldParser = tldParser;
            }
        }
    }
}
