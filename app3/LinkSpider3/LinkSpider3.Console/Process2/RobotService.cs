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
        private ConcurrentDictionary<string, List<string>> cache = new ConcurrentDictionary<string, List<string>>();
        #endregion

        static string user_agent = "Mozilla/5.0 (Windows NT 6.1; rv:15.0) Gecko/20100101 Firefox/15.0.1";

        public List<string> GetDenyUrls(string host)
        {
            if (cache.ContainsKey(host))
            {
                List<string> GetHost = new List<string>();
                cache.TryGetValue(host, out GetHost);
                return GetHost;
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
                                            RobotInstruction ri = new RobotInstruction(instructionLine);
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
                                                    if ((instructionLine.StartsWith("*") == true) || ((ri.UrlOrAgent.IndexOf(user_agent) >= 0)))
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


                cache.AddOrUpdate(host, deniedUrls, (s, l) => { return l;  });
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

        /// <summary>
        /// Use this class to read/parse the robots.txt file
        /// </summary>
        /// <remarks>
        /// Types of data coming into this class
        /// User-agent: * ==> _Instruction='User-agent', _Url='*'
        /// Disallow: /cgi-bin/ ==> _Instruction='Disallow', _Url='/cgi-bin/'
        /// Disallow: /tmp/ ==> _Instruction='Disallow', _Url='/tmp/'
        /// Disallow: /~joe/ ==> _Instruction='Disallow', _Url='/~joe/'
        /// </remarks>
        private class RobotInstruction
        {
            private string _Instruction;
            private string _Url = string.Empty;

            /// <summary>
            /// Constructor requires a line, hopefully in the format [instuction]:[url]
            /// </summary>
            public RobotInstruction(string line)
            {
                string instructionLine = line;
                int commentPosition = instructionLine.IndexOf('#');
                if (commentPosition == 0)
                {
                    _Instruction = "#";
                }
                if (commentPosition >= 0)
                {   // comment somewhere on the line, trim it off
                    instructionLine = instructionLine.Substring(0, commentPosition);
                }
                if (instructionLine.Length > 0)
                {   // wasn't just a comment line (which should have been filtered out before this anyway
                    string[] lineArray = instructionLine.Split(':');
                    _Instruction = lineArray[0].Trim().ToLower();
                    if (lineArray.Length > 1)
                    {
                        _Url = lineArray[1].Trim();
                    }
                }
            }
            /// <summary>
            /// Lower-case part of robots.txt line, before the colon (:)
            /// </summary>
            public string Instruction
            {
                get { return _Instruction; }
            }
            /// <summary>
            /// Lower-case part of robots.txt line, after the colon (:)
            /// </summary>
            public string UrlOrAgent
            {
                get { return _Url; }
            }
        }

        #region Disposable
        protected override void Cleanup()
        {
            //
        }
        #endregion
    }
}
