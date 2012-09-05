using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using LinkSpider3.Process2.Extensions;
using LinkSpider3.Process2.Utils;

namespace LinkSpider3.Process2
{
    public class VisitedDomains
        : Core.DisposableBase
    {
        private HashSet<UInt64> visitedDomainHistory = new HashSet<UInt64>();
        private ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        public readonly DateTime LogDate = DateTime.Now;

        public void Add(string url)
        {
            Uri uri = url.ToUri();
            if (!uri.IsNull())
            {
                if (!ContainsDomain(url))
                {
                    locker.EnterWriteLock();
                    try
                    {
                        visitedDomainHistory.Add(GetDomain(url).ToHash());
                    }
                    finally
                    {
                        locker.ExitWriteLock();
                    }
                }
            }
        }

        public bool ContainsDomain(string url)
        {
            Uri uri = url.ToUri();

            if (uri.IsNull())
            {
                return false;
            }
            else
            {
                locker.EnterReadLock();

                try
                {
                    return visitedDomainHistory.Contains(GetDomain(url).ToHash());
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }
        }

        public void Remove(string url)
        {
            Uri uri = url.ToUri();

            if (!uri.IsNull())
            {
                try
                {
                    locker.EnterWriteLock();
                    visitedDomainHistory.Remove(GetDomain(url).ToHash());
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
        }

        static string GetDomain(string url)
        {
            HtmlProcessor.LinkInfo li = new HtmlProcessor.LinkInfo(TldParser.Instance);
            li.Href = url;
            return li.Domain;
        }

        #region Disposable
        protected override void Cleanup()
        {
            locker.Dispose();
        }
        #endregion
    }
}
