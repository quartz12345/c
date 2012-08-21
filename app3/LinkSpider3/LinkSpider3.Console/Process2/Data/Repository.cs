using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

using LinkSpider3.Process2.Extensions;
using LinkSpider3.Process2.Persistence;

namespace LinkSpider3.Process2.Data
{
    public class Repository
    {
        public ConcurrentDictionary<string, LinkData> Links;
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

        public void LoadLinks()
        {
            //Links = this.persistence.Load<ConcurrentDictionary<string, LinkData>>(null) 
            //    as ConcurrentDictionary<string, LinkData>;
            Links = new ConcurrentDictionary<string, LinkData>();
        }

        public void SaveLink(
            string link, 
            string childLink, 
            string backLink,
            HtmlProcessor.LinkInfo linkInfo)
        {
            LinkData l;
            l = new LinkData();
            l.Link = link;

            if (!childLink.IsNullOrEmpty() && !l.ChildLinks.Contains(childLink))
                l.ChildLinks.Add(childLink);

            if (!backLink.IsNullOrEmpty() && !l.BackLinks.Contains(backLink))
                l.BackLinks.Add(backLink);

            //l = Links.Where(lk => { return lk.Link == link; }).FirstOrDefault();

            Links.AddOrUpdate(link, l,
                (key, oldLinkData) =>
                {
                    if (!childLink.IsNullOrEmpty() && !l.ChildLinks.Contains(childLink))
                        oldLinkData.ChildLinks.Add(childLink);

                    if (!backLink.IsNullOrEmpty() && !l.BackLinks.Contains(backLink))
                        oldLinkData.BackLinks.Add(backLink);

                    return oldLinkData;
                });

        }
    }
}
