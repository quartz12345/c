using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LinkSpider3.Process;

namespace LinkSpider3.Test
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class LinkRegistryTest
    {
        CollectorManager CM;

        public LinkRegistryTest()
        {
            this.CM = new CollectorManager();
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestInitializeTld()
        {
            LinkInfo u = new LinkInfo(CM);
            
            Assert.IsTrue(LinkInfo.TldMozillaList.Count > 0, "TldMozillaList not loaded");
            
            foreach (string tld in LinkInfo.TldMozillaList)
                if (tld.StartsWith("//"))
                    Assert.Fail("TldMozillaList contains a 'comment' value");
                else if (string.IsNullOrWhiteSpace(tld))
                    Assert.Fail("TldMozillaList contains an empty string");
        }

        [TestMethod]
        public void TestNewLinkInfo()
        {
            LinkInfo u = new LinkInfo(CM, "http://www.yahoo.com", "http://www.google.com", "Yahoo!", "nofollow", "anchor");
            Assert.IsTrue(u.Link == "http://www.yahoo.com/", "Link is wrong");
            Assert.IsTrue(u.Domain == "yahoo.com", "Domain is wrong `{0}`", u.Domain);
            Assert.IsTrue(u.DomainOrSubdomain == "www.yahoo.com", "DomainOrSubdomain is wrong `{0}`", u.DomainOrSubdomain);
            Assert.IsTrue(u.Tld == "com", "Tld is wrong `{0}`", u.Tld);
            Assert.IsTrue(u.DomainScheme == "http", "Scheme is wrong {0}", u.DomainScheme);
            Assert.IsTrue(u.DomainSchemeAndServer == "http://www.yahoo.com/", "SchemeAndServer is wrong {0}", u.DomainSchemeAndServer);
            Assert.IsTrue(!string.IsNullOrEmpty(u.DomainCountry), "Country is not resolved for {0}", u.Domain);
            Assert.IsTrue(u.IPAddresses.Count > 0, "No ip address is resolved for {0}", u.Domain);
            foreach (string s in u.RobotsExclusion)
            {
                Assert.IsTrue(s.StartsWith("/"), "Invalid robots.txt for {0}", u.DomainSchemeAndServer);
            }
            

            u = new LinkInfo(CM, "https://dev.mysql.com/", "http://www.google.com", "MySQL", "nofollow", "anchor");
            Assert.IsTrue(u.Link == "https://dev.mysql.com/", "Link is wrong `{0}`", u.Link);
            Assert.IsTrue(u.Domain == "mysql.com", "Domain is wrong `{0}`", u.Domain);
            Assert.IsTrue(u.DomainOrSubdomain == "dev.mysql.com", "DomainOrSubdomain is wrong `{0}`", u.DomainOrSubdomain);
            Assert.IsTrue(u.Tld == "com", "Tld is wrong `{0}`", u.Tld);
            Assert.IsTrue(u.DomainScheme == "https", "Scheme is wrong {0}", u.DomainScheme);
            Assert.IsTrue(u.DomainSchemeAndServer == "https://dev.mysql.com/", "SchemeAndServer is wrong {0}", u.DomainSchemeAndServer);
            Assert.IsTrue(!string.IsNullOrEmpty(u.DomainCountry), "Country is not resolved for {0}", u.Domain);
            Assert.IsTrue(u.IPAddresses.Count > 0, "No ip address is resolved for {0}", u.Domain);
            Assert.IsTrue(u.RobotsExclusion.Count > 0, "No robots.txt for {0}", u.DomainSchemeAndServer);

            u = new LinkInfo(CM, "http://www.google.com.ph/webhp?sourceid=chrome-instant", "http://www.google.com", "MySQL", "nofollow", "anchor");
            Assert.IsTrue(u.Link == "http://www.google.com.ph/webhp?sourceid=chrome-instant", "Link is wrong `{0}`", u.Link);
            Assert.IsTrue(u.Domain == "google.com.ph", "Domain is wrong `{0}`", u.Domain);
            Assert.IsTrue(u.DomainOrSubdomain == "www.google.com.ph", "DomainOrSubdomain is wrong `{0}`", u.DomainOrSubdomain);
            Assert.IsTrue(u.Tld == "com.ph", "Tld is wrong `{0}`", u.Tld);
            Assert.IsTrue(u.DomainScheme == "http", "Scheme is wrong {0}", u.DomainScheme);
            Assert.IsTrue(u.DomainSchemeAndServer == "http://www.google.com.ph/", "SchemeAndServer is wrong {0}", u.DomainSchemeAndServer);
            Assert.IsTrue(!string.IsNullOrEmpty(u.DomainCountry), "Country is not resolved for {0}", u.Domain);
            Assert.IsTrue(u.IPAddresses.Count > 0, "No ip address is resolved for {0}", u.Domain);
            Assert.IsTrue(u.RobotsExclusion.Count > 0, "No robots.txt for {0}", u.DomainSchemeAndServer);
        }
    }
}
