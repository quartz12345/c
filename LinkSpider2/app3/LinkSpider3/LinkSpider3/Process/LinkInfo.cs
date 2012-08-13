using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Web.Script.Serialization;

namespace LinkSpider3.Process
{
    public class LinkInfo
    {
        static LinkInfo()
        {
            InitializeRegistry();
        }

        public static List<string> TldMozillaList = new List<string>();
        static void InitializeRegistry()
        {
            string[] lines = Regex.Split(Properties.Resources.effective_tld_names_dat, "[\r\n]+");
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    if (lines[i].Substring(0, 2) != "//")
                    {
                        TldMozillaList.Add(lines[i]);
                    }
                }
            }
        }




        public string Link { get; set; }
        public string Tld { get; set; }
        public string Domain { get; set; }
        public string DomainOrSubdomain { get; set; }
        public List<string> IPAddresses { get; set; }
        public List<string> Backlinks { get; set; }
        public string DomainCountry { get; set; }
        public string DomainScheme { get; set; }
        public string DomainSchemeAndServer { get; set; }
        public string LinkPairID { get; set; }
        public bool LinkExcludedInRobots { get; set; }
        
        [ScriptIgnore]
        public AnchorInfo AnchorInfo { get; set; }
        
        [ScriptIgnore]
        public List<string> RobotsExclusion { get; set; }

        CollectorManager CM;

        public LinkInfo(CollectorManager cm)
        {
            this.Backlinks = new List<string>();
            this.CM = cm;
        }

        public LinkInfo(CollectorManager cm, string link)
            : this(cm, link, string.Empty, string.Empty, string.Empty, string.Empty)
        {
        }

        public LinkInfo(CollectorManager cm, string link, string backlink, string text, string rel, string kind)
            : this(cm)
        {
            RetrieveInfo(link, backlink, text, rel, kind);
        }



        public void Merge(LinkInfo info)
        {
            if (info.IPAddresses != null)
            {
                foreach (string ip in info.IPAddresses)
                {
                    if (!this.IPAddresses.Contains(ip))
                        this.IPAddresses.Add(ip);
                }
            }

            if (info.Backlinks != null)
            {
                foreach (string backlink in info.Backlinks)
                {
                    if (!this.Backlinks.Contains(backlink))
                        this.Backlinks.Add(backlink);
                }
            }
        }

        void RetrieveInfo(string link, string backlink, string text, string rel, string kind)
        {
            backlink = LinkParser.Validate(backlink, string.Empty);
            link = LinkParser.Validate(link, backlink);

            if (!string.IsNullOrEmpty(link))
            {
                UriBuilder uri = new UriBuilder(link);

                this.Link = uri.Uri.AbsoluteUri;
                this.Domain = uri.Uri.DnsSafeHost;
                this.DomainOrSubdomain = uri.Uri.DnsSafeHost;
                this.DomainScheme = uri.Scheme;
                this.DomainSchemeAndServer =
                    new Uri(uri.Uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped)).AbsoluteUri;

                if (!string.IsNullOrEmpty(backlink))
                {
                    this.Backlinks.Add(new Uri(backlink).AbsoluteUri);
                    this.LinkPairID = string.Concat(
                        this.Link, "***D***", new Uri(backlink).AbsoluteUri);
                }

                this.AnchorInfo = new AnchorInfo
                {
                    Kind = kind,
                    Rel = rel,
                    Text = text
                };

                ParseSubdomainAndTld();
                LoadAndCheckRobotsRule();
            }
        }

        void ParseSubdomainAndTld()
        {
            if (Uri.CheckHostName(this.Domain) != UriHostNameType.IPv4 &&
                Uri.CheckHostName(this.Domain) != UriHostNameType.IPv6)
            {
                // Get the TLD
                foreach (string tld in TldMozillaList)
                {
                    if (this.Domain.EndsWith(tld))
                    {
                        this.Tld = tld;
                        string domainNoTld = this.Domain.Replace("." + tld, string.Empty);

                        // Get the correct domain
                        int lastDotPos = domainNoTld.LastIndexOf('.');
                        this.Domain =
                            domainNoTld.Substring(lastDotPos + 1) + '.' + this.Tld;

                        break;
                    }
                }

                if (string.IsNullOrWhiteSpace(this.Tld))
                    this.DomainOrSubdomain = this.Domain;

                // Get the IP addresses & country
                IPAddresses = new List<string>();
                try
                {
                    Array.ForEach(Dns.GetHostEntry(this.DomainOrSubdomain).AddressList, ip =>
                    {
                        IPAddresses.Add(ip.ToString());

                        if (string.IsNullOrEmpty(this.DomainCountry))
                        {
                            // TODO: Too slow
                            //LocationInfo li = GeoLocationService.GetLocationInfo(ip.ToString());
                            //if (li != null)
                            //    this.DomainCountry = li.CountryCode;
                        }
                    });
                }
                catch { }

            }
        }


        protected virtual void LoadAndCheckRobotsRule()
        {
            RobotsCache cache = new RobotsCache(this.CM, this.DomainSchemeAndServer);
            RobotsExclusion = cache.RobotsExclusion;
            cache = null;
            
            foreach (string robotExclusion in this.RobotsExclusion)
            {
                Uri robotUri = new Uri(new Uri(this.DomainSchemeAndServer), robotExclusion);
                if (this.Link.StartsWith(robotUri.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    LinkExcludedInRobots = true;
                    break;
                }
            }
        }
    }
}
