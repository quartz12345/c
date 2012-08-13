using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LinkSpider3.Process;

namespace LinkSpider3.Test
{
    [TestClass]
    public class RobotsCacheTest
    {
        [TestMethod]
        public void TestRobotsCache()
        {
            CollectorManager cm = new CollectorManager();

            RobotsCache cache = new RobotsCache(cm, "http://www.yahoo.com");
            Assert.IsTrue(cache.RobotsExclusion.Count > 0, "Robots not loaded");
        }
    }
}
