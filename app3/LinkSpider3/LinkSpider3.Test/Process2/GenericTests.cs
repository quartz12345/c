using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using LinkSpider3.Process2;
using LinkSpider3.Process2.Extensions;

using HtmlAgilityPack;
using System.Net;


namespace LinkSpider3.Test.Process2
{
    [TestClass]
    public class GenericTests
    {
        [TestMethod]
        public void DownloadRobots()
        {
            using (RobotService rs = new RobotService())
            {
                Uri uri = new Uri("http://www.yahoo.com");
                Assert.IsTrue(rs.GetDenyUrls(uri.Host).Count > 0);
                Assert.IsTrue(rs.IsAllowed(string.Empty, uri));
                rs.GetDenyUrls(uri.Host).ForEach(denyUrl =>
                {
                    Console.WriteLine(denyUrl);
                });
            }
        }

        [TestMethod]
        public void ResolveDns()
        {
            Uri uri = new Uri("http://www.mysql.com/");
            Uri newUri = DnsResolver.Instance.Resolve(uri);
            
            Assert.IsTrue(newUri.Host.Replace(".", string.Empty).All(c =>
            {
                return Char.IsNumber(c);
            }));
        }

        [TestMethod]
        public void TldParserer()
        {
            Uri uri = new Uri("http://www.yahoo.com");
            TldParser parser = new TldParser();
            Assert.IsTrue(parser.GetTld(uri.Host) == "com");
        }

        [TestMethod]
        public void HtmlProcessorer()
        {
            Uri uri = new Uri("http://www.google.com");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Timeout = 500;
            request.CookieContainer = new CookieContainer();
            request.Method = "GET";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (!response.ResponseUri.Equals(uri))
                {
                    uri = response.ResponseUri;
                }

                HtmlProcessor processor = new HtmlProcessor(
                    uri.ToString(), response.GetResponseStream(),
                    new TldParser());
                Assert.IsTrue(!processor.Links.IsNull());
                Assert.IsTrue(processor.Links.Count > 0);
            }
        }

        [TestMethod]
        public void WebDownloaderer()
        {
            Uri uri = new Uri("http://lixam.com");

            WebDownloader web = new WebDownloader(uri, null, ea =>
            {
                Assert.IsNotNull(ea.Stream);
                ea.Stream.Close();
            });

            web.Download();
        }


        [TestMethod]
        public void CollectorStacker()
        {
            CollectorPool stack = new CollectorPool();
            Assert.IsTrue(stack.Next() == "google.com");
            Assert.IsTrue(stack.Next() == "facebook.com");
            stack.Store("jubacs.somee.com");
            Assert.IsTrue(stack.Next() == "jubacs.somee.com");
        }

        [TestMethod]
        public void ToUrii()
        {
            Assert.IsTrue("google.com".ToUri().ToString() == "http://www.google.com/");
            Assert.IsTrue("www.google.com".ToUri().ToString() == "http://www.google.com/");
            Assert.IsTrue("http://www.google.com".ToUri().ToString() == "http://www.google.com/");
        }
    }
}
