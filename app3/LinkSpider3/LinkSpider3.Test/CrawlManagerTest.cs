using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LinkSpider3.Process;

namespace LinkSpider3.Test
{
    [TestClass]
    public class CrawlManagerTest
    {
        [TestMethod]
        public void TestCrawlManagerStart()
        {
            CollectorManager mgr = new CollectorManager();
            mgr.Start("127.0.0.1");
        }
    }
}
