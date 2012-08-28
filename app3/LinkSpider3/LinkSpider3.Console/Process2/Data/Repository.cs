using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Net;

using LinkSpider3.Process2.Core;
using LinkSpider3.Process2.Extensions;
using LinkSpider3.Process2.Persistence;
using LinkSpider3.Process2.Utils;

namespace LinkSpider3.Process2.Data
{
    public class Repository
    {
        // Saved as urn:link:data
        // link, LinkData
        // public ConcurrentDictionary<string, LinkData> Links;
        public ConcurrentRedisHash<LinkData> Links;
        
        // Saved as urn:link:status:current
        // link, status (200, 500, etc.)
        //public ConcurrentDictionary<string, int> LinkStatusCurrent;
        public ConcurrentRedisHash<int> LinkStatusCurrent;

        // Saved as urn:link:status:history
        // link, array of LinkStatus
        //public ConcurrentDictionary<string, List<LinkStatus>> LinkStatusHistory;
        public ConcurrentRedisHash<List<LinkStatus>> LinkStatusHistory;

        // Saved as urn:link:rating
        // link, number of backlinks
        //public ConcurrentDictionary<string, int> LinkRating;
        public ConcurrentRedisHash<int> LinkRating;

        // Saved as urn:link:crawldate:current
        // link, LinkDate
        //public ConcurrentDictionary<string, LinkDate> LinkCrawlDateCurrent;
        public ConcurrentRedisHash<LinkDate> LinkCrawlDateCurrent;

        // Saved as urn:link:crawldate:history
        // link, array of LinkDate
        //public ConcurrentDictionary<string, List<LinkDate>> LinkCrawlDateHistory;
        public ConcurrentRedisHash<List<LinkDate>> LinkCrawlDateHistory;

        // Saved as urn:crawldate:link
        public ConcurrentRedisHash<List<string>> CrawlDateLinks;

        // Saved as urn:domain:data
        // domain, array of link
        //public ConcurrentDictionary<string, List<string>> Domains;
        public ConcurrentRedisHash<List<string>> Domains;

        // Saved as urn:domainorsubdomain:data
        // subdomain, array of link
        //public ConcurrentDictionary<string, List<string>> DomainOrSubdomains;
        public ConcurrentRedisHash<List<string>> DomainOrSubdomains;


        // Saved as urn:anchor:data
        // link_rfid + childlink_rfid, array of AnchorData
        //public ConcurrentDictionary<string, List<AnchorData>> Anchors;
        public ConcurrentRedisHash<List<AnchorData>> Anchors;

        // Saved as urn:anchor:textexact
        // text_rfid + link_rfid + childlink_rfid, array of AnchorDataLinkTextRelation
        //public ConcurrentDictionary<string, List<AnchorDataLinkTextRelation>> AnchorTextExactRelations;
        public ConcurrentRedisHash<List<AnchorDataLinkTextRelation>> AnchorTextExactRelations;



        private IPersistence persistence;

        public Repository(IPersistence persistence)
        {
            this.persistence = persistence;
        }

        public void Load(out CollectorPool pool)
        {
            pool = this.persistence.Load<CollectorPool>(null) as CollectorPool;
        }

        public void Load(out VisitedUrlHistory history, DateTime date)
        {
            history = new VisitedUrlHistory();
        }

        public void LoadData()
        {
            //Links = this.persistence.Load<ConcurrentDictionary<string, LinkData>>(null) as ConcurrentDictionary<string, LinkData>;
            //LinkStatusCurrent = this.persistence.Load<ConcurrentDictionary<string, int>>(null) as ConcurrentDictionary<string, int>;
            //LinkStatusHistory = this.persistence.Load<ConcurrentDictionary<string, List<LinkStatus>>>(null) as ConcurrentDictionary<string, List<LinkStatus>>;
            //LinkRating = this.persistence.Load<ConcurrentDictionary<string, int>>(null) as ConcurrentDictionary<string, int>;
            //LinkCrawlDateCurrent = this.persistence.Load<ConcurrentDictionary<string, LinkDate>>(null) as ConcurrentDictionary<string, LinkDate>;
            //LinkCrawlDateHistory = this.persistence.Load<ConcurrentDictionary<string, List<LinkDate>>>(null) as ConcurrentDictionary<string, List<LinkDate>>;
            //Domains = this.persistence.Load<ConcurrentDictionary<string, List<string>>>(null) as ConcurrentDictionary<string, List<string>>;
            //DomainOrSubdomains = this.persistence.Load<ConcurrentDictionary<string, List<string>>>(null) as ConcurrentDictionary<string, List<string>>;
            //Anchors = this.persistence.Load<ConcurrentDictionary<string, List<AnchorData>>>(null) as ConcurrentDictionary<string, List<AnchorData>>;
            //AnchorTextExactRelations = this.persistence.Load<ConcurrentDictionary<string, List<AnchorDataLinkTextRelation>>>(null) as ConcurrentDictionary<string, List<AnchorDataLinkTextRelation>>;

            Links = this.persistence.Load<ConcurrentRedisHash<LinkData>>(new Dictionary<string, object> { { "name", "urn:link:data" }, { "db", 1 } }) as ConcurrentRedisHash<LinkData>;
            LinkStatusCurrent = this.persistence.Load<ConcurrentRedisHash<int>>(new Dictionary<string, object> { { "name", "urn:link:status:current" }, { "db", 2 } }) as ConcurrentRedisHash<int>;
            LinkStatusHistory = this.persistence.Load<ConcurrentRedisHash<List<LinkStatus>>>(new Dictionary<string, object> { { "name", "urn:link:status:history" }, { "db", 3 } }) as ConcurrentRedisHash<List<LinkStatus>>;
            LinkRating = this.persistence.Load<ConcurrentRedisHash<int>>(new Dictionary<string, object> { { "name", "urn:link:rating" }, { "db", 4 } }) as ConcurrentRedisHash<int>;
            LinkCrawlDateCurrent = this.persistence.Load<ConcurrentRedisHash<LinkDate>>(new Dictionary<string, object> { { "name", "urn:link:crawldate:current" }, { "db", 5 } }) as ConcurrentRedisHash<LinkDate>;
            LinkCrawlDateHistory = this.persistence.Load<ConcurrentRedisHash<List<LinkDate>>>(new Dictionary<string, object> { { "name", "urn:link:crawldate:history" }, { "db", 6 } }) as ConcurrentRedisHash<List<LinkDate>>;
            CrawlDateLinks = this.persistence.Load<ConcurrentRedisHash<List<string>>>(new Dictionary<string, object> { { "name", "urn:crawldate:link" }, { "db", 7 } }) as ConcurrentRedisHash<List<string>>;
            Domains = this.persistence.Load<ConcurrentRedisHash<List<string>>>(new Dictionary<string, object> { { "name", "urn:domain:data" }, { "db", 8 } }) as ConcurrentRedisHash<List<string>>;
            DomainOrSubdomains = this.persistence.Load<ConcurrentRedisHash<List<string>>>(new Dictionary<string, object> { { "name", "urn:domainorsubdomain:data" }, { "db", 9 } }) as ConcurrentRedisHash<List<string>>;
            Anchors = this.persistence.Load<ConcurrentRedisHash<List<AnchorData>>>(new Dictionary<string, object> { { "name", "urn:anchor:data" }, { "db", 10 } }) as ConcurrentRedisHash<List<AnchorData>>;
            AnchorTextExactRelations = this.persistence.Load<ConcurrentRedisHash<List<AnchorDataLinkTextRelation>>>(new Dictionary<string, object> { { "name", "urn:anchor:textexact" }, { "db", 11 } }) as ConcurrentRedisHash<List<AnchorDataLinkTextRelation>>;
        }

        public void SaveLink(
            string link, 
            string childLink, 
            string backLink,
            HtmlProcessor.LinkInfo linkInfo)
        {
            // Save link data
            LinkData l = new LinkData();
            l.IsDirty = true;
            l.Link_RFID = link.ToHash().ToString();
            l.Link = link;
            l.Domain = linkInfo.Domain;
            l.DomainOrSubdomain = linkInfo.DomainOrSubdomain;
            l.DomainScheme = linkInfo.DomainScheme;
            l.Tld = linkInfo.Tld;
            l.IP = IP.GetIPAddress(l.DomainOrSubdomain);
            l.IPType = IP.GetIPClassFamily2(l.IP);
            l.Title = linkInfo.Title;

            if (!childLink.IsNullOrEmpty() && !l.ChildLinks.Contains(childLink))
            {
                l.ChildLinks.Add(childLink);
                l.NewChildLinks.Add(childLink);
            }

            if (!backLink.IsNullOrEmpty() && !l.BackLinks.Contains(backLink))
                l.BackLinks.Add(backLink);

            // Save links
            l = Links.AddOrUpdate(link.ToHashString(), l,
                (key, oldLinkData) =>
                {
                    return oldLinkData.Merge(l);
                });


            // Save link status current
            LinkStatusCurrent.AddOrUpdate(link.ToHashString(), linkInfo.Status,
                (key, oldStatus) =>
                {
                    return linkInfo.Status;
                });


            // Save link status history
            LinkStatus linkStatus = new LinkStatus
            {
                Link = link,
                Date = DateTime.Now.ToString("yyMMdd"),
                Time = DateTime.Now.ToString("hhmmss"),
                Status = linkInfo.Status
            };
            LinkStatusHistory.AddOrUpdate(link.ToHashString(), new List<LinkStatus> { linkStatus },
                (key, oldLinkStatuses) =>
                {
                    oldLinkStatuses.Add(linkStatus);
                    return oldLinkStatuses;
                });

            
            // Save link rating
            LinkRating.AddOrUpdate(link.ToHashString(), Links[link].BackLinks.Count,
                (key, oldCount) => { return Links[link].BackLinks.Count; });


            // Save link crawl date
            LinkDate linkDate = new LinkDate
            {
                Date = DateTime.Now.ToString("yyMMdd"),
                Time = DateTime.Now.ToString("hhmmss")
            };
            LinkCrawlDateCurrent.AddOrUpdate(link.ToHashString(), linkDate,
                (key, oldLinkDate) => { return linkDate; });


            // Save link crawl date history
            LinkCrawlDateHistory.AddOrUpdate(link.ToHashString(), new List<LinkDate> { linkDate },
                (key, oldLinkDates) =>
                {
                    oldLinkDates.Add(linkDate);
                    return oldLinkDates;
                });

            
            // Crawl date
            CrawlDateLinks.AddOrUpdate(DateTime.Today.ToString("yyMMdd"), new List<string> { link.ToHashString() },
                (key, oldList) =>
                {
                    if (!oldList.Contains(link.ToHashString()))
                        oldList.Add(link.ToHashString());
                    return oldList;
                });


            // Domains
            Domains.AddOrUpdate(l.Domain.ToHashString(), new List<string> { l.Link.ToHashString() },
                (key, oldLinks) =>
                {
                    if (!oldLinks.Contains(l.Link.ToHashString()))
                        oldLinks.Add(l.Link.ToHashString());
                    return oldLinks;
                });


            // DomainOrSubdomains
            DomainOrSubdomains.AddOrUpdate(l.DomainOrSubdomain.ToHashString(), new List<string> { l.Link.ToHashString() },
                (key, oldLinks) =>
                {
                    if (!oldLinks.Contains(l.Link.ToHashString()))
                        oldLinks.Add(l.Link.ToHashString());
                    return oldLinks;
                });


            // AnchorData
            if (!childLink.IsNullOrEmpty())
            {
                AnchorData anchorData = new AnchorData
                {
                    Kind = linkInfo.AnchorKind,
                    Rel = linkInfo.AnchorRel,
                    Text = linkInfo.AnchorText
                };
                Anchors.AddOrUpdate(link.ToHash() + "_" + childLink.ToHash(), new List<AnchorData> { anchorData },
                    (key, oldAnchors) =>
                    {
                        if (!oldAnchors.Contains(anchorData))
                            oldAnchors.Add(anchorData);
                        return oldAnchors;
                    });

                // AnchorTextExactRelations
                AnchorDataLinkTextRelation relation = new AnchorDataLinkTextRelation
                {
                    AnchorText = linkInfo.AnchorText,
                    Link = link,
                    ChildLink = childLink,
                    LinkChildLinkRelation_RFID =
                        link.ToHash() + "_" +
                        childLink.ToHash()
                };

                AnchorTextExactRelations.AddOrUpdate(
                    linkInfo.AnchorText.ToHash() + "_" +
                    link.ToHash() + "_" +
                    childLink.ToHash(), new List<AnchorDataLinkTextRelation> { relation },
                    (key, oldRelations) =>
                    {
                        if (!oldRelations.Contains(relation))
                            oldRelations.Add(relation);
                        return oldRelations;
                    });
            }

        }

    }
}
