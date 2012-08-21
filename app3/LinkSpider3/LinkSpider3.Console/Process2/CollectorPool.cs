using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

using LinkSpider3.Process2.Extensions;
using System.Text.RegularExpressions;

namespace LinkSpider3.Process2
{
    public class CollectorPool : Core.DisposableBase
    {
        private ConcurrentQueue<string> pool;
        private int currentTop1MCounter = 0;
        private string[] top1MLinks;

        public CollectorPool()
            : this(null)
        {
        }

        public CollectorPool(IEnumerable<string> pool)
        {
            currentTop1MCounter = Convert.ToInt32(Properties.Settings.Default.Top1MCounter);

            if (pool.IsNull())
                this.pool = new ConcurrentQueue<string>();
            else
                this.pool = new ConcurrentQueue<string>(pool);
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
                if (top1MLinks.IsNull())
                    top1MLinks = Regex.Split(Properties.Resources.top_1m, "[\r\n]+");

                string link = top1MLinks[currentTop1MCounter].Split(',')[1];

                ++currentTop1MCounter;

                return link;
            }
        }

        public int Count
        {
            get { return pool.Count; }
        }

        #region Disposable
        protected override void Cleanup()
        {
            Properties.Settings.Default["Top1MCounter"] = currentTop1MCounter;
        }
        #endregion
    }
}
