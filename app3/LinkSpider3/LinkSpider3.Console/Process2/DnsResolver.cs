using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using LinkSpider3.Process2.Extensions;

namespace LinkSpider3.Process2
{
    public class DnsResolver
    {
        private Dictionary<ulong, Uri> cache = new Dictionary<ulong, Uri>();

        private DnsResolver() { }
        private static object o = new object();
        private static DnsResolver instance;
        public static DnsResolver Instance
        {
            get
            {
                lock (o)
                {
                    if (instance == null)
                        instance = new DnsResolver();
                    return instance;
                }
            }
        }

        public Uri Resolve(Uri uri)
        {
            ulong authority = uri.Host.ToHash();
            
            if (cache.ContainsKey(authority))
                return cache[authority];


            string host;

            IPHostEntry hostEntry = Dns.GetHostEntry(uri.Host);
            IPAddress[] ips = hostEntry.AddressList;
            if (ips.Length > 0)
                host = ips[0].ToString();
            else
                host = uri.Host;

            UriBuilder b = new UriBuilder(uri);
            b.Host = host;

            cache.Add(authority, b.Uri);
            
            return b.Uri;
        }
    }
}
