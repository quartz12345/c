using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Threading;
using System.Diagnostics;
//using ServiceStack.Redis;
using TeamDev.Redis;

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
        //IRedisClient Redis;
        RedisDataAccessProvider Redis;
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
                lock (o)
                {
                    SaveLink(this);
                }

                if (crawlChildLinks)
                    CrawlChildLinks();
            }
        }

        // ~Collector() { if (this.Redis != null) this.Redis.Dispose(); }


        static void SaveLink(Collector c)
        {
            //IRedisHash urnLinkData = this.Redis.Hashes["urn:link:data"];
            var urnLinkData = c.Redis.Hash["urn:link:data"];
            if (urnLinkData.ContainsKey(c.Link))
            {
                string serializedLinkData = urnLinkData[c.Link];
                c.LinkInfo.Merge(serializedLinkData.JsonDeserialize<LinkInfo>());
            }

            //me.Redis.SetEntryInHash("urn:link:data", 
            //    me.Link, me.LinkInfo.JsonSerialize());
            c.Redis.Hash["urn:link:data"][c.Link] = c.LinkInfo.JsonSerialize();
            
            //me.Redis.SetEntryInHash("urn:link:data-last-date-crawl",
            //    me.Link, DateTime.Now.ToString());
            c.Redis.Hash["urn:link:data-last-date-crawl"][c.Link] = DateTime.Now.ToString();

            // Index date last crawl
            //IRedisHash urnLinkDateLastCrawl = me.Redis.Hashes["urn:link:date-last-crawl"];
            
            // Seperate redis connection seems to be fixing race conditions
            using (RedisDataAccessProvider myRedis = new RedisDataAccessProvider())
            {
                myRedis.Configuration = c.Redis.Configuration;

                var urnLinkDateLastCrawl = myRedis.Hash["urn:link:date-last-crawl"];
                List<string> dateLastCrawlLinks = new List<string>();
                if (urnLinkDateLastCrawl.ContainsKey(DateTime.Today.ToString()))
                    dateLastCrawlLinks = urnLinkDateLastCrawl[DateTime.Today.ToString()]
                        .JsonDeserialize<List<string>>();
                else
                    dateLastCrawlLinks = new List<string>();
                if (!dateLastCrawlLinks.Contains(c.Link))
                    dateLastCrawlLinks.Add(c.Link);
                //me.Redis.SetEntryInHash("urn:link:date-last-crawl",
                //    DateTime.Today.ToString(), dateLastCrawlLinks.JsonSerialize());
                urnLinkDateLastCrawl.Set(DateTime.Today.ToString(), dateLastCrawlLinks.JsonSerialize());

                myRedis.Close();
            }


            if (!string.IsNullOrEmpty(c.CurrentBacklink))
            {
                // Index anchor
                //IRedisHash urnLinkAnchor = me.Redis.Hashes["urn:link:anchor"];
                var urnLinkAnchor = c.Redis.Hash["urn:link:anchor"];
                List<string> anchorData;
                if (urnLinkAnchor.ContainsKey(c.LinkInfo.LinkPairID))
                    anchorData = urnLinkData[c.LinkInfo.LinkPairID]
                        .JsonDeserialize<List<string>>();
                else
                    anchorData = new List<string>();
                if (!anchorData.Contains(c.LinkInfo.AnchorInfo.JsonSerialize()))
                {
                    anchorData.Add(c.LinkInfo.AnchorInfo.JsonSerialize());
                    //me.Redis.SetEntryInHash("urn:link:anchor",
                    //    me.LinkInfo.LinkPairID, anchorData.JsonSerialize());
                    urnLinkAnchor[c.LinkInfo.LinkPairID] = anchorData.JsonSerialize();
                }

                //Redis.Lists["urn:link:anchor:" + me.LinkInfo.LinkPairID].RemoveValue(me.LinkInfo.AnchorInfo.JsonSerialize());
                //Redis.Lists["urn:link:anchor:" + me.LinkInfo.LinkPairID].Append(me.LinkInfo.AnchorInfo.JsonSerialize());

                // TODO: Index external backlinks
                //foreach (string backlink in me.LinkInfo.Backlinks)
                //{
                    //Redis.Lists["urn:backlink-external-link:" + backlink.Replace(':','_')].RemoveValue(me.Link);
                    //Redis.Lists["urn:backlink-external-link:" + backlink.Replace(':', '_')].Append(me.Link);
                //}

                // Index backlink count by link
                //me.Redis.SetEntryInHash("urn:link:backlink-count",
                //    me.Link, me.LinkInfo.Backlinks.Count.ToString());
                c.Redis.Hash["urn:link:backlink-count"][c.Link] =
                    c.LinkInfo.Backlinks.Count.ToString();
            }


            // Index domain or subdomain
            //IRedisHash urnLinkDomainOrSubdomain = me.Redis.Hashes["urn:link:domain-or-subdomain"];
            var urnLinkDomainOrSubdomain = c.Redis.Hash["urn:link:domain-or-subdomain"];
            List<string> domainOrSubdomainLinks;
            if (urnLinkDomainOrSubdomain.ContainsKey(c.LinkInfo.DomainOrSubdomain))
                domainOrSubdomainLinks = urnLinkDomainOrSubdomain[c.LinkInfo.DomainOrSubdomain]
                    .JsonDeserialize<List<string>>();
            else
                domainOrSubdomainLinks = new List<string>();
            if (!domainOrSubdomainLinks.Contains(c.Link))
                domainOrSubdomainLinks.Add(c.Link);
            //me.Redis.SetEntryInHash("urn:link:domain-or-subdomain",
            //    me.LinkInfo.DomainOrSubdomain, domainOrSubdomainLinks.JsonSerialize());
            urnLinkDomainOrSubdomain[c.LinkInfo.DomainOrSubdomain] =
                domainOrSubdomainLinks.JsonSerialize();


            // Index domain
            //IRedisHash urnLinkDomain = me.Redis.Hashes["urn:link:domain"];
            var urnLinkDomain = c.Redis.Hash["urn:link:domain"];
            List<string> domainLinks;
            if (urnLinkDomain.ContainsKey(c.LinkInfo.Domain))
                domainLinks = urnLinkDomain[c.LinkInfo.Domain]
                    .JsonDeserialize<List<string>>();
            else
                domainLinks = new List<string>();
            if (!domainLinks.Contains(c.Link))
                domainLinks.Add(c.Link);
            //me.Redis.SetEntryInHash("urn:link:domain",
            //    me.LinkInfo.Domain, domainLinks.JsonSerialize());
            urnLinkDomain[c.LinkInfo.Domain] = domainLinks.JsonSerialize();

            //Redis.Lists["urn:link:domain-or-subdomain:" + me.LinkInfo.DomainOrSubdomain].RemoveValue(me.Link);
            //Redis.Lists["urn:link:domain-or-subdomain:" + me.LinkInfo.DomainOrSubdomain].Append(me.Link);
            //Redis.Lists["urn:link:domain:" + me.LinkInfo.Domain].RemoveValue(me.Link);
            //Redis.Lists["urn:link:domain:" + me.LinkInfo.Domain].Append(me.Link);
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
            //this.Redis.SetEntryInHash("urn:link:status", this.Link, log.JsonSerialize());
            this.Redis.Hash["urn:link:status"][this.Link] = log.JsonSerialize();

            
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
                                    //this.Redis.RemoveItemFromList("urn:pool", crawler.Link);
                                    //this.Redis.AddItemToList("urn:pool", crawler.Link);
                                    this.Redis.List["urn:pool"].Remove(crawler.Link);

                                    if (this.CM.COLLECTOR_DIRECTION == CollectorManager.COLLECTOR_DIRECTION_OLDEST)
                                        this.Redis.List["urn:pool"].Append(crawler.Link);

                                    List<string> childLinks = this.Redis.Hash["urn:link-child:data"][backlink]
                                        .JsonDeserialize<List<string>>();
                                    if (!childLinks.Contains(crawler.Link))
                                        childLinks.Add(crawler.Link);
                                    this.Redis.Hash["urn:link-child:data"][backlink] = childLinks.JsonSerialize();

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
                //this.Redis.RemoveItemFromList("urn:pool", this.Link);
                //this.Redis.AddItemToList("urn:pool", this.Link);
                this.Redis.List["urn:pool"].Remove(this.Link);
                this.Redis.List["urn:pool"].Append(this.Link);

                Thread.Sleep(5000);
                this.CM.LinksAccessing.Remove(this.Link);
                this.CM.LinksAccessing.Add(this.Link + " [" + client.StatusCode + "] ");
                Thread.Sleep(5000);
                this.CM.LinksAccessing.Remove(this.Link + " [" + client.StatusCode + "] ");
            }
        }
    }
}
