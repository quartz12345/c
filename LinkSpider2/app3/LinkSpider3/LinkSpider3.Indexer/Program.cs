using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Redis;

namespace LinkSpider3.Indexer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(DateTime.Today.ToString());

            Console.Write("Index 1=Crawl Date, 2=Domain/Subdomain: ");
            char choice = Convert.ToChar(Console.Read());

            using (RedisClient Redis = new RedisClient())
            {
                switch (choice)
                {
                    case '1':
                        IndexCrawlDate(Redis);
                        break;
                    case '2':
                        IndexDomainOrSubdomain(Redis);
                        break;
                }
            }

            Console.WriteLine("fin!");
            Console.Read();
        }

        private static void IndexCrawlDate(RedisClient Redis)
        {
            List<string> linkList = Redis.GetHashKeys("urn:link:data-last-date-crawl");
            int i = 0;
            Array.ForEach(linkList.ToArray(), link =>
            {
                i++;
                Console.WriteLine(i + " of " + linkList.Count);

                string dateValue = Redis.GetValueFromHash("urn:link:data-last-date-crawl", link);

                string serializedLinks = Redis.GetValueFromHash("urn:link:date-last-crawl",
                    Convert.ToDateTime(dateValue).Date.ToString());

                List<string> links;
                if (string.IsNullOrEmpty(serializedLinks))
                    links = new List<string>();
                else
                    links = serializedLinks.JsonDeserialize<List<string>>();

                if (!links.Contains(link))
                {
                    links.Add(link);
                    Redis.SetEntryInHash("urn:link:date-last-crawl",
                        Convert.ToDateTime(dateValue).Date.ToString(), links.JsonSerialize());
                }
            });
        }

        private static void IndexDomainOrSubdomain(RedisClient Redis)
        {
            List<string> keys = Redis.SearchKeys("urn:link:domain-or-subdomain:*");
            for (int i = 0; i < keys.Count; i++)
            {
                List<string> links = Redis.GetAllItemsFromList(keys[i]);

                string domainOrSubdomain = keys[0].Substring(29);

                Console.WriteLine(i + ": " + domainOrSubdomain);

                Redis.SetEntryInHash("urn:link:domain-or-subdomain",
                    domainOrSubdomain, links.JsonSerialize());
            }
        }
    }
}
