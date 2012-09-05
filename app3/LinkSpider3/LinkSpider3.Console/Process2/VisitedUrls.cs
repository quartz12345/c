using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LinkSpider3.Process2.Extensions;

namespace LinkSpider3.Process2
{
    public class VisitedUrls
        : Core.DisposableBase
    {
        private HashSet<UInt64> visitedUrlHistory = new HashSet<UInt64>();
        private ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        public readonly DateTime LogDate = DateTime.Now;

        public void Add(string url)
        {
            if (!ContainsUrl(url))
            {
                locker.EnterWriteLock();
                try
                {
                    visitedUrlHistory.Add(url.ToHash());
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
        }

        public bool ContainsUrl(string url)
        {
            locker.EnterReadLock();

            try
            {
                return visitedUrlHistory.Contains(url.ToHash());
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        #region Disposable
        protected override void Cleanup()
        {
            locker.Dispose();
        }
        #endregion
    }
}
