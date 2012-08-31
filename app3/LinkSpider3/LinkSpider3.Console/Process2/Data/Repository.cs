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
        public IConcurrentHash<LinkData> Links;
        
        // Saved as urn:link:status:current
        // link, status (200, 500, etc.)
        //public ConcurrentDictionary<string, int> LinkStatusCurrent;
        public IConcurrentHash<SimpleObject<int>> LinkStatusCurrent;

        // Saved as urn:link:status:history
        // link, array of LinkStatus
        //public ConcurrentDictionary<string, List<LinkStatus>> LinkStatusHistory;
        public IConcurrentHash<SimpleObject<List<LinkStatus>>> LinkStatusHistory;

        // Saved as urn:link:rating
        // link, number of backlinks
        //public ConcurrentDictionary<string, int> LinkRating;
        public IConcurrentHash<SimpleObject<int>> LinkRating;

        // Saved as urn:link:crawldate:current
        // link, LinkDate
        //public ConcurrentDictionary<string, LinkDate> LinkCrawlDateCurrent;
        public IConcurrentHash<SimpleObject<LinkDate>> LinkCrawlDateCurrent;

        // Saved as urn:link:crawldate:history
        // link, array of LinkDate
        //public ConcurrentDictionary<string, List<LinkDate>> LinkCrawlDateHistory;
        public IConcurrentHash<SimpleObject<List<LinkDate>>> LinkCrawlDateHistory;

        // Saved as urn:crawldate:link
        public IConcurrentHash<SimpleObject<List<string>>> CrawlDateLinks;

        // Saved as urn:domain:data
        // domain, array of link
        //public ConcurrentDictionary<string, List<string>> Domains;
        public IConcurrentHash<SimpleObject<DomainData>> Domains;

        // Saved as urn:domainorsubdomain:data
        // subdomain, array of link
        //public ConcurrentDictionary<string, List<string>> DomainOrSubdomains;
        public IConcurrentHash<SimpleObject<DomainOrSubdomainData>> DomainOrSubdomains;


        // Saved as urn:anchor:data
        // link_rfid + childlink_rfid, array of AnchorData
        //public ConcurrentDictionary<string, List<AnchorData>> Anchors;
        public IConcurrentHash<SimpleObject<List<AnchorData>>> Anchors;

        // Saved as urn:anchor:textexact
        // text_rfid + link_rfid + childlink_rfid, array of AnchorDataLinkTextRelation
        //public ConcurrentDictionary<string, List<AnchorDataLinkTextRelation>> AnchorTextExactRelations;
        public IConcurrentHash<SimpleObject<List<AnchorDataLinkTextRelation>>> AnchorTextExactRelations;



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

            //if (this.persistence.ProviderName == "redis")
            //{
            //    Links = this.persistence.Load<ConcurrentRedisHash<LinkData>>(new Dictionary<string, object> { { "name", "urn:link:data" }, { "db", 1 } }) as IConcurrentHash<LinkData>;
            //    LinkStatusCurrent = this.persistence.Load<ConcurrentRedisHash<int>>(new Dictionary<string, object> { { "name", "urn:link:status:current" }, { "db", 2 } }) as IConcurrentHash<int>;
            //    LinkStatusHistory = this.persistence.Load<ConcurrentRedisHash<List<LinkStatus>>>(new Dictionary<string, object> { { "name", "urn:link:status:history" }, { "db", 3 } }) as IConcurrentHash<List<LinkStatus>>;
            //    LinkRating = this.persistence.Load<ConcurrentRedisHash<int>>(new Dictionary<string, object> { { "name", "urn:link:rating" }, { "db", 4 } }) as IConcurrentHash<int>;
            //    LinkCrawlDateCurrent = this.persistence.Load<ConcurrentRedisHash<LinkDate>>(new Dictionary<string, object> { { "name", "urn:link:crawldate:current" }, { "db", 5 } }) as IConcurrentHash<LinkDate>;
            //    LinkCrawlDateHistory = this.persistence.Load<ConcurrentRedisHash<List<LinkDate>>>(new Dictionary<string, object> { { "name", "urn:link:crawldate:history" }, { "db", 6 } }) as IConcurrentHash<List<LinkDate>>;
            //    CrawlDateLinks = this.persistence.Load<ConcurrentRedisHash<List<string>>>(new Dictionary<string, object> { { "name", "urn:crawldate:link" }, { "db", 7 } }) as IConcurrentHash<List<string>>;
            //    Domains = this.persistence.Load<ConcurrentRedisHash<List<string>>>(new Dictionary<string, object> { { "name", "urn:domain:data" }, { "db", 8 } }) as IConcurrentHash<List<string>>;
            //    DomainOrSubdomains = this.persistence.Load<ConcurrentRedisHash<List<string>>>(new Dictionary<string, object> { { "name", "urn:domainorsubdomain:data" }, { "db", 9 } }) as IConcurrentHash<List<string>>;
            //    Anchors = this.persistence.Load<ConcurrentRedisHash<List<AnchorData>>>(new Dictionary<string, object> { { "name", "urn:anchor:data" }, { "db", 10 } }) as IConcurrentHash<List<AnchorData>>;
            //    AnchorTextExactRelations = this.persistence.Load<ConcurrentRedisHash<List<AnchorDataLinkTextRelation>>>(new Dictionary<string, object> { { "name", "urn:anchor:textexact" }, { "db", 11 } }) as IConcurrentHash<List<AnchorDataLinkTextRelation>>;
            //}

            if (this.persistence.ProviderName == "mongodb")
            {
                Links = this.persistence.Load<ConcurrentMongoHash<LinkData>>(new Dictionary<string, object> { { "name", "urn:link:data" }, { "db", 1 } }) as IConcurrentHash<LinkData>;
                LinkStatusCurrent = this.persistence.Load<ConcurrentMongoHash<SimpleObject<int>>>(new Dictionary<string, object> { { "name", "urn:link:status:current" }, { "db", 2 } }) as IConcurrentHash<SimpleObject<int>>;
                LinkStatusHistory = this.persistence.Load < ConcurrentMongoHash<SimpleObject<List<LinkStatus>>>>(new Dictionary<string, object> { { "name", "urn:link:status:history" }, { "db", 3 } }) as IConcurrentHash<SimpleObject<List<LinkStatus>>>;
                LinkRating = this.persistence.Load < ConcurrentMongoHash<SimpleObject<int>>>(new Dictionary<string, object> { { "name", "urn:link:rating" }, { "db", 4 } }) as IConcurrentHash<SimpleObject<int>>;
                LinkCrawlDateCurrent = this.persistence.Load<ConcurrentMongoHash<SimpleObject<LinkDate>>>(new Dictionary<string, object> { { "name", "urn:link:crawldate:current" }, { "db", 5 } }) as IConcurrentHash<SimpleObject<LinkDate>>;
                LinkCrawlDateHistory = this.persistence.Load<ConcurrentMongoHash<SimpleObject<List<LinkDate>>>>(new Dictionary<string, object> { { "name", "urn:link:crawldate:history" }, { "db", 6 } }) as IConcurrentHash<SimpleObject<List<LinkDate>>>;
                CrawlDateLinks = this.persistence.Load<ConcurrentMongoHash<SimpleObject<List<string>>>>(new Dictionary<string, object> { { "name", "urn:crawldate:link" }, { "db", 7 } }) as IConcurrentHash<SimpleObject<List<string>>>;
                Domains = this.persistence.Load<ConcurrentMongoHash<SimpleObject<DomainData>>>(new Dictionary<string, object> { { "name", "urn:domain:data" }, { "db", 8 } }) as IConcurrentHash<SimpleObject<DomainData>>;
                DomainOrSubdomains = this.persistence.Load<ConcurrentMongoHash<SimpleObject<DomainOrSubdomainData>>>(new Dictionary<string, object> { { "name", "urn:domainorsubdomain:data" }, { "db", 9 } }) as IConcurrentHash<SimpleObject<DomainOrSubdomainData>>;
                Anchors = this.persistence.Load<ConcurrentMongoHash<SimpleObject<List<AnchorData>>>>(new Dictionary<string, object> { { "name", "urn:anchor:data" }, { "db", 10 } }) as IConcurrentHash<SimpleObject<List<AnchorData>>>;
                AnchorTextExactRelations = this.persistence.Load<ConcurrentMongoHash<SimpleObject<List<AnchorDataLinkTextRelation>>>>(new Dictionary<string, object> { { "name", "urn:anchor:textexact" }, { "db", 11 } }) as IConcurrentHash<SimpleObject<List<AnchorDataLinkTextRelation>>>;
            }
        }


        public void SaveLink(
            string link, 
            string childLink, 
            string backLink,
            HtmlProcessor.LinkInfo linkInfo)
        {
            // Save link data
            LinkData l = new LinkData();
            //l.IsDirty = true;
            l.Id = link.ToHashString();
            l.Link = link;
            l.Domain = linkInfo.Domain;
            //l.DomainOrSubdomain = linkInfo.DomainOrSubdomain;
            l.DomainScheme = linkInfo.DomainScheme;
            l.Tld = linkInfo.Tld;
            l.IP = IP.GetIPAddress(l.Domain);
            l.IPType = IP.GetIPClassFamily2(l.IP);
            l.Title = linkInfo.Title;

            if (!childLink.IsNullOrEmpty() && !l.ChildLinks.Contains(childLink))
            {
                l.ChildLinks.Add(childLink);
                //l.NewChildLinks.Add(childLink);
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
            SimpleObject<int> so1 = new SimpleObject<int>
            {
                Id = l.Id,
                Value = linkInfo.Status
            };
            LinkStatusCurrent.AddOrUpdate(so1.Id, so1,
                (key, oldSo1) =>
                {
                    return so1;
                });


            // Save link status history
            LinkStatus linkStatus = new LinkStatus
            {
                //Link = link,
                Date = DateTime.Now.ToString("yyMMdd"),
                Time = DateTime.Now.ToString("hhmmss"),
                Status = linkInfo.Status
            };
            SimpleObject<List<LinkStatus>> so2 = new SimpleObject<List<LinkStatus>>
            {
                Id = l.Id,
                Value = new List<LinkStatus> { linkStatus }
            };
            LinkStatusHistory.AddOrUpdate(so2.Id, so2,
                (key, oldSo2) =>
                {
                    oldSo2.Value.Add(linkStatus);
                    return oldSo2;
                });

            
            // Save link rating
            SimpleObject<int> so3 = new SimpleObject<int>
            {
                Id = l.Id,
                Value = Links[l.Id].BackLinks.Count 
            };
            LinkRating.AddOrUpdate(so3.Id, so3,
                (key, oldSo3) => 
                { 
                    return so3; 
                });


            // Save link crawl date
            LinkDate linkDate = new LinkDate
            {
                Date = DateTime.Now.ToString("yyMMdd"),
                Time = DateTime.Now.ToString("hhmmss")
            };
            SimpleObject<LinkDate> so4 = new SimpleObject<LinkDate>
            {
                Id = l.Id,
                Value = linkDate
            };
            LinkCrawlDateCurrent.AddOrUpdate(so4.Id, so4,
                (key, oldSo4) => 
                { 
                    return so4; 
                });


            // Save link crawl date history
            SimpleObject<List<LinkDate>> so5 = new SimpleObject<List<LinkDate>>
            {
                Id = l.Id,
                Value = new List<LinkDate> { linkDate }
            };
            LinkCrawlDateHistory.AddOrUpdate(so5.Id, so5,
                (key, oldSo5) =>
                {
                    oldSo5.Value.Add(linkDate);
                    return oldSo5;
                });

            
            // Crawl date
            SimpleObject<List<string>> so6 = new SimpleObject<List<string>>
            {
                Id = DateTime.Today.ToString("yyMMdd"),
                Value = new List<string> { l.Id }
            };
            CrawlDateLinks.AddOrUpdate(so6.Id, so6,
                (key, oldSo6) =>
                {
                    if (!oldSo6.Value.Contains(l.Id))
                        oldSo6.Value.Add(l.Id);
                    return oldSo6;
                });


            // Domains
            SimpleObject<DomainData> so7 = new SimpleObject<DomainData>
            {
                Id = l.Domain.ToHashString(),
                Value = new DomainData { Domain = l.Domain, Links = new List<string> { l.Id } }
            };
            Domains.AddOrUpdate(so7.Id, so7,
                (key, oldSo7) =>
                {
                    if (oldSo7.Value.IsNull())
                        oldSo7.Value = new DomainData { Domain = l.Domain, Links = new List<string>() };

                    if (!oldSo7.Value.Links.Contains(l.Id))
                        oldSo7.Value.Links.Add(l.Id);
                    return oldSo7;
                });


            // DomainOrSubdomains
            //SimpleObject<DomainOrSubdomainData> so7a = new SimpleObject<DomainOrSubdomainData>
            //{
            //    Id = l.DomainOrSubdomain.ToHashString(),
            //    Value = new DomainOrSubdomainData { DomainOrSubdomain = l.DomainOrSubdomain, Links = new List<string> { l.Id } }
            //};
            //DomainOrSubdomains.AddOrUpdate(so7a.Id, so7a,
            //    (key, oldSo7a) =>
            //    {
            //        if (oldSo7a.Value.IsNull())
            //            oldSo7a.Value = new DomainOrSubdomainData { DomainOrSubdomain = l.DomainOrSubdomain, Links = new List<string> { l.Id } };

            //        if (!oldSo7a.Value.Links.Contains(l.Id))
            //            oldSo7a.Value.Links.Add(l.Id);
            //        return oldSo7a;
            //    });


            // AnchorData
            if (!childLink.IsNullOrEmpty())
            {
                AnchorData anchorData = new AnchorData
                {
                    Kind = linkInfo.AnchorKind,
                    Rel = linkInfo.AnchorRel,
                    Text = linkInfo.AnchorText
                };
                SimpleObject<List<AnchorData>> so8 = new SimpleObject<List<AnchorData>>
                {
                    Id = (link + childLink).ToHashString(),
                    Value = new List<AnchorData> { anchorData }
                };
                Anchors.AddOrUpdate(so8.Id, so8,
                    (key, oldSo8) =>
                    {
                        if (!oldSo8.Value.Contains(anchorData))
                            oldSo8.Value.Add(anchorData);
                        return oldSo8;
                    });

                // AnchorTextExactRelations
                AnchorDataLinkTextRelation relation = new AnchorDataLinkTextRelation
                {
                    AnchorText = linkInfo.AnchorText,
                    Link = link,
                    ChildLink = childLink,
                    LinkChildLinkRelation_RFID = (link + childLink).ToHashString()
                };
                SimpleObject<List<AnchorDataLinkTextRelation>> so9 = new SimpleObject<List<AnchorDataLinkTextRelation>>
                {
                    Id = (linkInfo.AnchorText + link + childLink).ToHashString(),
                    Value = new List<AnchorDataLinkTextRelation> { relation }
                };

                AnchorTextExactRelations.AddOrUpdate(so9.Id, so9,
                    (key, oldSo9) =>
                    {
                        if (!oldSo9.Value.Contains(relation))
                            oldSo9.Value.Add(relation);
                        return oldSo9;
                    });
            }

        }



        public void Commit<T>(T o, IDictionary<string, object> properties)
        {
            this.persistence.Save<T>(o, properties);
        }

    }
}
