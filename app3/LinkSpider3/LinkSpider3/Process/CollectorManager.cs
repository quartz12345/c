using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.ComponentModel;
using LinkSpider3.Hack;
using System.Configuration;
using ServiceStack.Redis;

namespace LinkSpider3.Process
{
    public class CollectorManager
    {

        internal class LinkCollectorTaskPair
        {
            public Task Task { get; set; }
            public LinkInfo LinkInfo { get; set; }
        }


        public List<string> LinksCurrentlyProcessing { get; private set; }
        public List<int> CollectorCount { get; private set; }
        public List<string> LinksAccessing { get; private set; }
        internal Dictionary<string, LinkCollectorTaskPair> Collectors;
        internal IRedisClient Redis;

        BasicRedisClientManager PRCM;
        int COLLECTOR_COUNT = 2;
        System.Timers.Timer CrawlNextLinkQueueManager = null;
        System.Timers.Timer CollectorsManager = null;
        System.Timers.Timer PoolManager = null;

        public CollectorManager()
        {
            Collectors = new Dictionary<string, LinkCollectorTaskPair>();
            
            LinksCurrentlyProcessing = new List<string>();
            LinksAccessing = new List<string>();
            
            CollectorCount = new List<int>();
            CollectorCount.Add(0);
        }
        
        ~CollectorManager() 
        {
            if (this.Redis != null) this.Redis.Dispose();
            if (this.PRCM != null) this.PRCM.Dispose();
        }


        public void Start(string redisServer)
        {
            // Initialize and start collectors only once
            if (this.Redis == null)
            {
                //this.PRCM = new BasicRedisClientManager(redisServer);
                this.Redis = new RedisClient(redisServer);

                if (CrawlNextLinkQueueManager == null)
                {
                    CrawlNextLinkQueueManager = new System.Timers.Timer(2000);
                    CrawlNextLinkQueueManager.Elapsed += (o, ea) =>
                    {
                        CrawlNextLinkFromPool(this);
                    };
                }
                CrawlNextLinkQueueManager.Start();

                //CrawlNextLinkFromPool(this);

                if (CollectorsManager == null)
                {
                    CollectorsManager = new System.Timers.Timer(500);
                    CollectorsManager.Elapsed += (o, ea) =>
                    {
                        LinkCollectorTaskPair workerToTerminate = null;
                        foreach (var worker in Collectors.Values)
                        {
                            if (worker.Task.IsCompleted ||
                                worker.Task.IsFaulted ||
                                worker.Task.IsCanceled)
                            {
                                workerToTerminate = worker;
                                break;
                            }
                        }

                        if (workerToTerminate != null)
                        {
                            workerToTerminate.Task.Dispose();
                            Collectors.Remove(workerToTerminate.LinkInfo.Link);
                            LinksCurrentlyProcessing.Remove(workerToTerminate.LinkInfo.Link);
                            CollectorCount[0] = Collectors.Count;
                        }
                    };
                }
                CollectorsManager.Start();


                //if (PoolManager == null)
                //{
                //    PoolManager = new System.Timers.Timer(10000);
                //    PoolManager.Elapsed += (o, ea) =>
                //    {
                //        PoolTop100SitesThatHaveBacklinksGreaterThan10(this);
                //    };
                //}
                //PoolManager.Stop();
                //PoolManager.Start();
            }
        }

        public void Stop()
        {
            if (CrawlNextLinkQueueManager != null)
            {
                CrawlNextLinkQueueManager.Stop();
                CrawlNextLinkQueueManager.Dispose();
            }

            if (CollectorsManager != null)
            {
                CollectorsManager.Stop();
                CollectorsManager.Dispose();
            }

            if (PoolManager != null)
            {
                PoolManager.Stop();
                PoolManager.Dispose();
            }
        }

        public void AddUrl(string url)
        {
            try
            {
                url = new Uri(url).AbsoluteUri;
                Redis.RemoveItemFromList("urn:pool", url);
                Redis.PrependItemToList("urn:pool", url);
            }
            catch { }
        }

        public void SetWorkerCount(int count)
        {
            COLLECTOR_COUNT = count;
        }

        public void ReIndex()
        {
        }

        #region Helpers
        internal bool IsDomainOrSubdomainCurrentlyCrawled(string domainOrSubdomain)
        {
            var q = from w in Collectors.Values
                    where w.LinkInfo.DomainOrSubdomain == domainOrSubdomain
                    select w;
            return q.Count() > 0;
        }
        #endregion
        
        #region Threads
        static void PoolTop100SitesThatHaveBacklinksGreaterThan10(object o)
        {
            //CrawlerManager m = (CrawlerManager)o;
            //KeyValuePair<string, string>[] pairs = 
            //    m.Redis.Hashes["urn:link:backlink-count"];
            //foreach (KeyValuePair<string, string> kp in pairs)
            //{
            //    int backlinkCount = Convert.ToInt32(kp.Value);
            //    if (backlinkCount > 10)
            //    {
            //        // Remove last crawl date to force crawling it again
            //        m.Redis.Hash["urn:link:data-last-date-crawl"].Delete(kp.Key);

            //        // Add to pool
            //        m.Redis.List["urn:pool"].Append(kp.Key);
            //    }
            //}
        }

        static void CrawlNextLinkFromPool(object o)
        {
            CollectorManager m = (CollectorManager)o;

            // Limit workers
            if (m.LinksCurrentlyProcessing.Count < m.COLLECTOR_COUNT)
            {
                string link = m.Redis.DequeueItemFromList("urn:pool");
                link = LinkParser.Validate(link, string.Empty);

                if (!string.IsNullOrEmpty(link))
                {
                    m.LinksCurrentlyProcessing.Add(link);

                    LinkInfo info = new LinkInfo(m, link);

                    if (m.IsDomainOrSubdomainCurrentlyCrawled(info.DomainOrSubdomain))
                    {
                        Thread.Sleep(500);
                        m.Redis.AddItemToList("urn:pool", link);
                        m.LinksCurrentlyProcessing.Remove(link);
                    }
                    else
                    {
                        DateTime? lastDateCrawl = null;
                        if (m.Redis.HashContainsEntry("urn:link:data-last-date-crawl", info.Link))
                        {
                            lastDateCrawl = Convert.ToDateTime(
                                m.Redis.GetValueFromHash("urn:link:data-last-date-crawl", info.Link));
                        }

                        if (!lastDateCrawl.HasValue ||
                            (DateTime.Now - lastDateCrawl.Value).Days > 30)
                        {
                            // Only crawl if the link has not been crawled since
                            // And if the last crawl was 30 days ago
                            m.Collectors.Add(link,
                                new LinkCollectorTaskPair
                                {
                                    Task = Task.Factory.StartNew(CrawlLinkInfo,
                                        new object[] { m, info }),
                                    LinkInfo = info
                                });
                            m.CollectorCount[0] = m.Collectors.Count;
                        }
                        else
                        {
                            m.LinksCurrentlyProcessing.Remove(link);
                        }
                    }
                }
            }
        }

        static void CrawlLinkInfo(object p)
        {
            object[] arguments = (object[])p;
            CollectorManager m = (CollectorManager)arguments[0];
            LinkInfo info = (LinkInfo)arguments[1];

            try
            {
                Collector crawler = Collector.CrawlAndSave(
                    m,
                    info.Link,
                    string.Empty, string.Empty, string.Empty, string.Empty, true, true);
                crawler = null;
            }
            catch { }
        }
        #endregion
    }
}
