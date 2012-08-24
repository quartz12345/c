using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Net;

using LinkSpider3.Process2.Extensions;
using LinkSpider3.Process2.Persistence;
using LinkSpider3.Process2.Utils;

namespace LinkSpider3.Process2.Data
{
    public class Repository
    {
        // Saved as urn:link:data
        // link, LinkData
        public ConcurrentDictionary<string, LinkData> Links;
        
        // Saved as urn:link:status:current
        // link, status (200, 500, etc.)
        public ConcurrentDictionary<string, int> LinkStatusCurrent;

        // Saved as urn:link:status:history
        // link, array of LinkStatus
        public ConcurrentDictionary<string, List<LinkStatus>> LinkStatusHistory;

        // Saved as urn:link:rating
        // link, number of backlinks
        public ConcurrentDictionary<string, int> LinkRating;

        // Saved as urn:link:crawldate
        // link, LinkDate
        public ConcurrentDictionary<string, LinkDate> LinkCrawlDateCurrent;

        // Saved as urn:link:crawldate:history
        // link, array of LinkDate
        public ConcurrentDictionary<string, List<LinkDate>> LinkCrawlDateHistory;

        // Saved as urn:domain:data
        // domain, array of link
        public ConcurrentDictionary<string, List<string>> Domains;

        // Saved as urn:domainorsubdomain:data
        // subdomain, array of link
        public ConcurrentDictionary<string, List<string>> DomainOrSubdomains;


        // Saved as urn:anchor:data
        // link_rfid + childlink_rfid, array of AnchorData
        public ConcurrentDictionary<string, List<AnchorData>> Anchors;

        // Saved as urn:anchor:textexact
        // text_rfid + link_rfid + childlink_rfid, array of AnchorDataLinkTextRelation
        public ConcurrentDictionary<string, List<AnchorDataLinkTextRelation>> AnchorTextExactRelations;



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
            Links = this.persistence.Load<ConcurrentDictionary<string, LinkData>>(null) as ConcurrentDictionary<string, LinkData>;
            LinkStatusCurrent = this.persistence.Load<ConcurrentDictionary<string, int>>(null) as ConcurrentDictionary<string, int>;
            LinkStatusHistory = this.persistence.Load<ConcurrentDictionary<string, List<LinkStatus>>>(null) as ConcurrentDictionary<string, List<LinkStatus>>;
            LinkRating = this.persistence.Load<ConcurrentDictionary<string, int>>(null) as ConcurrentDictionary<string, int>;
            LinkCrawlDateCurrent = this.persistence.Load<ConcurrentDictionary<string, LinkDate>>(null) as ConcurrentDictionary<string, LinkDate>;
            LinkCrawlDateHistory = this.persistence.Load<ConcurrentDictionary<string, List<LinkDate>>>(null) as ConcurrentDictionary<string, List<LinkDate>>;
            Domains = this.persistence.Load<ConcurrentDictionary<string, List<string>>>(null) as ConcurrentDictionary<string, List<string>>;
            DomainOrSubdomains = this.persistence.Load<ConcurrentDictionary<string, List<string>>>(null) as ConcurrentDictionary<string, List<string>>;
            Anchors = this.persistence.Load<ConcurrentDictionary<string, List<AnchorData>>>(null) as ConcurrentDictionary<string, List<AnchorData>>;
            AnchorTextExactRelations = this.persistence.Load<ConcurrentDictionary<string, List<AnchorDataLinkTextRelation>>>(null) as ConcurrentDictionary<string, List<AnchorDataLinkTextRelation>>;
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
            l.Link_RFID = link.ToRabinFingerPrint();
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

            l = Links.AddOrUpdate(link, l,
                (key, oldLinkData) =>
                {
                    return oldLinkData.Merge(l);
                });


            // Save link status current
            LinkStatusCurrent.AddOrUpdate(link, linkInfo.Status,
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
            LinkStatusHistory.AddOrUpdate(link, new List<LinkStatus> { linkStatus },
                (key, oldLinkStatuses) =>
                {
                    oldLinkStatuses.Add(linkStatus);
                    return oldLinkStatuses;
                });

            
            // Save link rating
            LinkRating.AddOrUpdate(link, Links[link].BackLinks.Count,
                (key, oldCount) => { return Links[link].BackLinks.Count; });


            // Save link crawl date
            LinkDate linkDate = new LinkDate
            {
                Date = DateTime.Now.ToString("yyMMdd"),
                Time = DateTime.Now.ToString("hhmmss")
            };
            LinkCrawlDateCurrent.AddOrUpdate(link, linkDate,
                (key, oldLinkDate) => { return linkDate; });


            // Save link crawl date history
            LinkCrawlDateHistory.AddOrUpdate(link, new List<LinkDate> { linkDate },
                (key, oldLinkDates) =>
                {
                    oldLinkDates.Add(linkDate);
                    return oldLinkDates;
                });


            // Domains
            Domains.AddOrUpdate(l.Domain, new List<string> { l.Link },
                (key, oldLinks) =>
                {
                    if (!oldLinks.Contains(l.Link))
                        oldLinks.Add(l.Link);
                    return oldLinks;
                });


            // DomainOrSubdomains
            DomainOrSubdomains.AddOrUpdate(l.DomainOrSubdomain, new List<string> { l.Link },
                (key, oldLinks) =>
                {
                    if (!oldLinks.Contains(l.Link))
                        oldLinks.Add(l.Link);
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
                Anchors.AddOrUpdate(link.ToRabinFingerPrint() + "_" + childLink.ToRabinFingerPrint(), new List<AnchorData> { anchorData },
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
                        link.ToRabinFingerPrint() + "_" +
                        childLink.ToRabinFingerPrint()
                };

                AnchorTextExactRelations.AddOrUpdate(
                    linkInfo.AnchorText.ToRabinFingerPrint() + "_" +
                    link.ToRabinFingerPrint() + "_" +
                    childLink.ToRabinFingerPrint(), new List<AnchorDataLinkTextRelation> { relation },
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
