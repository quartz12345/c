using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Configuration;
using ServiceStack.Redis;
using System.Threading;
using System.Diagnostics;

namespace LinkSpider3.Process
{
    public class Collector
    {
        public static Collector CrawlAndSave(
            CollectorManager cm,
            string link, string backlink, string text, string rel, string kind,
            bool crawlChildLinks, bool poolChildLinksFound)
        {

            link = LinkParser.Validate(link, backlink);
            if (string.IsNullOrEmpty(link))
            {
                // Do nothing, unparsable url
                return null;
            }
            else
            {
                Collector collector = new Collector(
                    cm, link, backlink, text, rel, kind, crawlChildLinks, poolChildLinksFound);
                return collector;
            }
        }




        public string Link { get; set; }
        public LinkInfo LinkInfo { get; set; }
        public string CurrentBacklink { get; set; }
        public bool PoolChildLinksFound { get; set; }

        CollectorManager CM;
        IRedisClient Redis;
        static object o = new object();

        Collector(
            CollectorManager cm,
            string link, string backlink, string text, string rel, string kind,
            bool crawlChildLinks, bool poolChildLinksFound)
        {
            this.CM = cm;
            this.Redis = cm.Redis;
            this.Link = link;
            this.CurrentBacklink = LinkParser.Validate(backlink, string.Empty);
            this.LinkInfo = new LinkInfo(cm, this.Link, this.CurrentBacklink, text, rel, kind);
            if (!this.LinkInfo.LinkExcludedInRobots)
            {
                SaveLink();

                if (crawlChildLinks)
                    CrawlChildLinks();
            }
        }

        // ~Collector() { if (this.Redis != null) this.Redis.Dispose(); }


        void SaveLink()
        {
            IRedisHash urnLinkData = this.Redis.Hashes["urn:link:data"];
            if (urnLinkData.ContainsKey(this.Link))
            {
                string serializedLinkData = urnLinkData[this.Link];
                this.LinkInfo.Merge(serializedLinkData.JsonDeserialize<LinkInfo>());
            }

            this.Redis.SetEntryInHash("urn:link:data", 
                this.Link, this.LinkInfo.JsonSerialize());
            
            this.Redis.SetEntryInHash("urn:link:data-last-date-crawl",
                this.Link, DateTime.Now.ToString());

            // Index date last crawl
            IRedisHash urnLinkDateLastCrawl = this.Redis.Hashes["urn:link:date-last-crawl"];
            List<string> dateLastCrawlLinks;
            if (urnLinkDateLastCrawl.ContainsKey(DateTime.Today.ToString()))
                dateLastCrawlLinks = urnLinkDateLastCrawl[DateTime.Today.ToString()]
                    .JsonDeserialize<List<string>>();
            else
                dateLastCrawlLinks = new List<string>();
            if (!dateLastCrawlLinks.Contains(this.Link))
                dateLastCrawlLinks.Add(this.Link);
            this.Redis.SetEntryInHash("urn:link:date-last-crawl",
                DateTime.Today.ToString(), dateLastCrawlLinks.JsonSerialize());


            if (!string.IsNullOrEmpty(this.CurrentBacklink))
            {
                // Index anchor
                IRedisHash urnLinkAnchor = this.Redis.Hashes["urn:link:anchor"];
                List<string> anchorData;
                if (urnLinkAnchor.ContainsKey(this.LinkInfo.LinkPairID))
                    anchorData = urnLinkData[this.LinkInfo.LinkPairID]
                        .JsonDeserialize<List<string>>();
                else
                    anchorData = new List<string>();
                if (!anchorData.Contains(this.LinkInfo.AnchorInfo.JsonSerialize()))
                {
                    anchorData.Add(this.LinkInfo.AnchorInfo.JsonSerialize());
                    this.Redis.SetEntryInHash("urn:link:anchor",
                        this.LinkInfo.LinkPairID, anchorData.JsonSerialize());
                }

                //Redis.Lists["urn:link:anchor:" + this.LinkInfo.LinkPairID].RemoveValue(this.LinkInfo.AnchorInfo.JsonSerialize());
                //Redis.Lists["urn:link:anchor:" + this.LinkInfo.LinkPairID].Append(this.LinkInfo.AnchorInfo.JsonSerialize());

                // TODO: Index external backlinks
                //foreach (string backlink in this.LinkInfo.Backlinks)
                //{
                    //Redis.Lists["urn:backlink-external-link:" + backlink.Replace(':','_')].RemoveValue(this.Link);
                    //Redis.Lists["urn:backlink-external-link:" + backlink.Replace(':', '_')].Append(this.Link);
                //}

                // Index backlink count by link
                this.Redis.SetEntryInHash("urn:link:backlink-count",
                    this.Link, this.LinkInfo.Backlinks.Count.ToString());
            }


            // Index domain or subdomain
            IRedisHash urnLinkDomainOrSubdomain = this.Redis.Hashes["urn:link:domain-or-subdomain"];
            List<string> domainOrSubdomainLinks;
            if (urnLinkDomainOrSubdomain.ContainsKey(this.LinkInfo.DomainOrSubdomain))
                domainOrSubdomainLinks = urnLinkDomainOrSubdomain[this.LinkInfo.DomainOrSubdomain]
                    .JsonDeserialize<List<string>>();
            else
                domainOrSubdomainLinks = new List<string>();
            if (!domainOrSubdomainLinks.Contains(this.Link))
                domainOrSubdomainLinks.Add(this.Link);
            this.Redis.SetEntryInHash("urn:link:domain-or-subdomain",
                this.LinkInfo.DomainOrSubdomain, domainOrSubdomainLinks.JsonSerialize());


            // Index domain
            IRedisHash urnLinkDomain = this.Redis.Hashes["urn:link:domain"];
            List<string> domainLinks;
            if (urnLinkDomain.ContainsKey(this.LinkInfo.Domain))
                domainLinks = urnLinkDomain[this.LinkInfo.Domain]
                    .JsonDeserialize<List<string>>();
            else
                domainLinks = new List<string>();
            if (!domainLinks.Contains(this.Link))
                domainLinks.Add(this.Link);
            this.Redis.SetEntryInHash("urn:link:domain",
                this.LinkInfo.Domain, domainLinks.JsonSerialize());

            //Redis.Lists["urn:link:domain-or-subdomain:" + this.LinkInfo.DomainOrSubdomain].RemoveValue(this.Link);
            //Redis.Lists["urn:link:domain-or-subdomain:" + this.LinkInfo.DomainOrSubdomain].Append(this.Link);
            //Redis.Lists["urn:link:domain:" + this.LinkInfo.Domain].RemoveValue(this.Link);
            //Redis.Lists["urn:link:domain:" + this.LinkInfo.Domain].Append(this.Link);
        }

        void CrawlChildLinks()
        {
            HtmlWeb client = new HtmlWeb();

            this.CM.LinksAccessing.Add(this.Link);
            HtmlDocument doc = null;

            
            // Load the document if possible
            try
            {
                doc = client.Load(this.Link);
            }
            catch { }

            
            // Log status
            LinkLog log = new LinkLog 
            { 
                DateCrawled = DateTime.Now, 
                StatusCode = (int)client.StatusCode 
            };
            this.Redis.SetEntryInHash("urn:link:status", this.Link, log.JsonSerialize());

            
            // Parse the urls
            if (client.StatusCode == HttpStatusCode.OK && doc != null)
            {
                HtmlNodeCollection anchors = doc.DocumentNode.SelectNodes("//a[@href]");
                int i = 0; 
                int count = anchors.Count;
                if (anchors != null && count > 0)
                {
                    foreach (var anchor in anchors)
                    {
                        string backlink = LinkParser.Validate(this.Link, string.Empty);
                        string link = anchor.GetAttributeValue("href", string.Empty);
                        link = LinkParser.Validate(link, backlink);

                        if (!string.IsNullOrEmpty(link))
                        {
                            string text = anchor.InnerHtml;
                            string rel = anchor.GetAttributeValue("rel", "dofollow");
                            string kind = "text";


                            // TODO: Change behavior to crawl only if not yet crawled
                            // If already crawled, check if it is past 30 days
                            //LinkInfo linkInfo = new LinkInfo(link, backlink, text, rel, kind);
                            //LinkInfo backlinkInfo = new LinkInfo(backlink);

                            i++;
                            string linkAccessedMsg = string.Format("{0} ({1} of {2}) -> {3}",
                                backlink, i, count, link);
                            //string linkAccessedMsg = link;


                            // Send msg to UI
                            lock (o)
                            {
                                if (string.IsNullOrEmpty(linkAccessedMsg))
                                    Debug.Fail("msg is null");
                                else
                                    this.CM.LinksAccessing.Add(linkAccessedMsg);
                            }

                            try
                            {
                                Collector crawler = Collector.CrawlAndSave(
                                    this.CM,
                                    link, backlink, text, rel, kind, false, false);

                                if (crawler != null)
                                {
                                    this.Redis.RemoveItemFromList("urn:pool", crawler.Link);
                                    this.Redis.AddItemToList("urn:pool", crawler.Link);
                                    crawler = null;
                                }
                            }
                            catch { }

                            // Send msg to UI
                            lock (o)
                            {
                                if (!string.IsNullOrEmpty(linkAccessedMsg))
                                    this.CM.LinksAccessing.Remove(linkAccessedMsg);
                            }
                        }
                    }
                }

                this.CM.LinksAccessing.Remove(this.Link);
            }
            else
            {
                // Add to the bottom of the pool
                this.Redis.RemoveItemFromList("urn:pool", this.Link);
                this.Redis.AddItemToList("urn:pool", this.Link);

                Thread.Sleep(5000);
                this.CM.LinksAccessing.Remove(this.Link);
                this.CM.LinksAccessing.Add(this.Link + " [" + client.StatusCode + "] ");
                Thread.Sleep(5000);
                this.CM.LinksAccessing.Remove(this.Link + " [" + client.StatusCode + "] ");
            }
        }
    }
}
