using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;

using LinkSpider3.Process2.Extensions;

namespace LinkSpider3.Process2
{
    public class RobotService
        : Core.DisposableBase
    {
        #region Cache
        private ConcurrentDictionary<string, List<string>> cache =
            new ConcurrentDictionary<string, List<string>>();
        #endregion

        public List<string> GetDenyUrls(string host)
        {
            if (cache.ContainsKey(host))
            {
                return cache[host];
            }
            else
            {
                return DownloadRobots(host);
            }
        }

        private List<string> DownloadRobots(string host)
        {
            List<string> deniedUrls = new List<string>();
            Uri resolvedUri = new Uri(string.Format("http://{0}/", host));

            using (ManualResetEvent done = new ManualResetEvent(false))
            {
                try
                {

                    WebDownloader web = new WebDownloader(
                        string.Format("http://{0}/robots.txt", resolvedUri.Host).ToUri(),
                        null,
                        ea =>
                        {
                            if (!ea.Stream.IsNull())
                            {
                                using (StreamReader sr = new StreamReader(ea.Stream))
                                {
                                    bool rulesApply = false;

                                    while (sr.Peek() >= 0)
                                    {
                                        string instructionLine = sr.ReadLine().ToUpperInvariant();
                                        if (!instructionLine.IsNullOrEmpty())
                                        {
                                            int commentPosition = instructionLine.IndexOf("#");

                                            if (commentPosition > -1)
                                                instructionLine = instructionLine.Substring(0, commentPosition);

                                            if (instructionLine.Length > 0)
                                            {
                                                if (instructionLine.StartsWith("U"))
                                                {
                                                    // User-agent: *
                                                    int colonPosition = instructionLine.IndexOf(":");
                                                    instructionLine = instructionLine.Substring(colonPosition + 1).Trim();
                                                    if (instructionLine.StartsWith("*"))
                                                        rulesApply = true;
                                                    else
                                                        rulesApply = false;
                                                }
                                                else if (instructionLine.StartsWith("D"))
                                                {
                                                    // Disallow: /
                                                    // Disallow: /cgi-bin
                                                    if (rulesApply)
                                                    {
                                                        int colonPosition = instructionLine.IndexOf(":");
                                                        instructionLine = instructionLine.Substring(colonPosition + 1).Trim();
                                                        Uri possibleDenyUri;
                                                        if (Uri.TryCreate(resolvedUri,
                                                            instructionLine, out possibleDenyUri))
                                                        {
                                                            if (!deniedUrls.Contains(possibleDenyUri.AbsoluteUri.ToUpperInvariant()))
                                                                deniedUrls.Add(possibleDenyUri.AbsoluteUri.ToUpperInvariant());
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    sr.Close();
                                }
                            }

                            done.Set();
                        });

                    web.Download();
                    done.WaitOne();
                }
                catch
                {
                    // Do nothing for now
                }


                cache.AddOrUpdate(host, deniedUrls, 
                    (s, l) => { return l;  });
                return deniedUrls;
            }
        }


        public bool IsAllowed(string userAgent, Uri uri)
        {
            var q = 
                from deniedUrl in GetDenyUrls(uri.Host)
                where
                    uri.AbsoluteUri.Length >= deniedUrl.Length &&
                    uri.AbsoluteUri.ToUpperInvariant().Substring(0, deniedUrl.Length) == deniedUrl
                select deniedUrl;

            return q.Count() < 1;
        }


        #region Disposable
        protected override void Cleanup()
        {
            //
        }
        #endregion
    }
}
