using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using LinkSpider3.Process2.Extensions;

namespace LinkSpider3.Process2
{
    public class VisitedDomainHistory
        : Core.DisposableBase
    {
        private HashSet<UInt64> visitedDomainHistory = new HashSet<UInt64>();
        private ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        public readonly DateTime LogDate = DateTime.Now;

        public void Add(string url)
        {
            string host = url.ToUri().Host;

            if (!ContainsHost(url))
            {
                locker.EnterWriteLock();
                try
                {
                    visitedDomainHistory.Add(host.ToRabinFingerPrint());
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
        }

        public bool ContainsHost(string url)
        {
            string host = url.ToUri().Host;
            locker.EnterReadLock();

            try
            {
                return visitedDomainHistory.Contains(host.ToRabinFingerPrint());
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
