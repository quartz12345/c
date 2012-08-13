using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Configuration;
using ServiceStack.Redis;

namespace LinkSpider3.Process
{
    public class RobotsCache
    {
        public string DomainSchemeAndServer;
        public List<string> RobotsExclusion;
        public string RobotID;
        public string RobotLastDateCrawlID;

        IRedisClient Redis;

        public RobotsCache(CollectorManager cm, string domainSchemeAndServer)
        {
            this.Redis = cm.Redis;
            this.DomainSchemeAndServer = new Uri(domainSchemeAndServer).AbsoluteUri;
            this.RobotID = "urn:domain:robots:data";
            this.RobotLastDateCrawlID = "urn:domain:robots:last-date-crawl";

            if (!TryRetrieveFromCache())
            {
                RetrieveRobots();
                this.Redis.SetEntryInHash(this.RobotID, this.DomainSchemeAndServer,
                    this.RobotsExclusion.JsonSerialize());
                this.Redis.SetEntryInHash(this.RobotLastDateCrawlID,this.DomainSchemeAndServer,
                    DateTime.Now.ToString());
            }
        }

        //~RobotsCache() { if (this.Redis != null) this.Redis.Dispose(); }


        bool TryRetrieveFromCache()
        {
            DateTime? lastCrawl = null;
            if (this.Redis.HashContainsEntry(this.RobotLastDateCrawlID, this.DomainSchemeAndServer))
                lastCrawl = Convert.ToDateTime(
                    this.Redis.GetValueFromHash(this.RobotLastDateCrawlID, this.DomainSchemeAndServer));

            if (lastCrawl.HasValue)
            {
                TimeSpan elapsed = DateTime.Now - lastCrawl.Value;
                if (elapsed.Days > 30)
                {
                    // Retrieve it from the fresh list
                    return false;
                }
                else
                {
                    RobotsExclusion = 
                        this.Redis.GetValueFromHash(this.RobotID, this.DomainSchemeAndServer)
                            .JsonDeserialize<List<string>>();

                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        void RetrieveRobots()
        {
            RobotsExclusion = new List<string>();

            if (!string.IsNullOrEmpty(this.DomainSchemeAndServer))
            {
                string url = new Uri(new Uri(this.DomainSchemeAndServer), "/robots.txt").AbsoluteUri;

                using (WebClient r = new WebClient())
                {
                    string robots = string.Empty;

                    try
                    {
                        robots = r.DownloadString(url);
                    }
                    catch { }

                    if (!string.IsNullOrEmpty(robots))
                    {
                        string[] lines = Regex.Split(robots, "[\r\n]+");
                        for (int i = 0; i < lines.Length; i++)
                        {
                            string line = lines[i];
                            if (Regex.IsMatch(line, @"^user-agent:\s+\*\s*$", RegexOptions.IgnoreCase))
                            {
                                int j;
                                for (j = i + 1; j < lines.Length; j++)
                                {
                                    if (string.IsNullOrEmpty(lines[j]) ||
                                        Regex.IsMatch(lines[j], @"^user-agent:\s+[\w\W\s]+$", RegexOptions.IgnoreCase))
                                        break;

                                    if (Regex.IsMatch(lines[j], @"^disallow:\s+", RegexOptions.IgnoreCase))
                                    {
                                        RobotsExclusion.Add(lines[j].Replace("Disallow:", string.Empty).Trim());
                                    }
                                }

                                i = j;
                            }
                        }
                    }
                }
            }
        }
    }
}
