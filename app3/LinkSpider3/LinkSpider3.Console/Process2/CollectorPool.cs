using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Configuration;

using LinkSpider3.Process2.Extensions;

namespace LinkSpider3.Process2
{
    public class CollectorPool : Core.DisposableBase
    {
        private ConcurrentQueue<string> pool;
        private int currentTop1MCounter = 0;
        private string[] top1MLinks;

        public string[] ExcludedDomains;

        public CollectorPool() : this(null) { }

        public CollectorPool(IEnumerable<string> pool)
        {
            currentTop1MCounter = Convert.ToInt32(Properties.Settings.Default.Top1MCounter);
            
            var l = new List<string>();
            Array.ForEach(ConfigurationManager.AppSettings["ExcludedDomains"].Split(new []{','}, StringSplitOptions.RemoveEmptyEntries), 
                excludedDomain =>
                {
                    l.Add(excludedDomain.Trim());
                });

            ExcludedDomains = l.ToArray();


            if (pool.IsNull())
                this.pool = new ConcurrentQueue<string>();
            else
            {
                IEnumerable<string> pools = pool.Where(
                    s =>
                    {
                        HtmlProcessor.LinkInfo li = new HtmlProcessor.LinkInfo(TldParser.Instance);
                        li.Href = s;
                        bool isExcluded = false;

                        foreach (var excludedDomain in ExcludedDomains)
                        {
                            if (li.Domain.IndexOf(excludedDomain) > -1)
                            {
                                isExcluded = true;
                                break;
                            }
                        }

                        return !isExcluded;
                    });

                this.pool = new ConcurrentQueue<string>(pools);
            }
        }

        public void Store(string url)
        {
            pool.Enqueue(url);
        }

        public string Next()
        {
            string value;

            if (this.pool.TryDequeue(out value))
            {
                return value;
            }
            else
            {
                //if (top1MLinks.IsNull())
                //    top1MLinks = Regex.Split(Properties.Resources.top_1m, "[\r\n]+");

                //string link = top1MLinks[currentTop1MCounter].Split(',')[1];

                //++currentTop1MCounter;

                //return link;
                return string.Empty;
            }
        }

        public int Count
        {
            get { return pool.Count; }
        }

        public string[] ToArray() { return pool.ToArray(); }

        #region Disposable
        protected override void Cleanup()
        {
            Properties.Settings.Default["Top1MCounter"] = currentTop1MCounter;
        }
        #endregion
    }
}
