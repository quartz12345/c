using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using Newtonsoft.Json;

namespace LinkSpider3.Process2.Data
{
    public class LinkData
    {
        public List<string> ChildLinks { get; set; }
        public List<string> BackLinks { get; set; }
        public string Link_RFID { get; set; }
        public string Link { get; set; }
        public string Domain { get; set; }
        public string DomainOrSubdomain { get; set; }
        public string DomainScheme { get; set; }
        public string Tld { get; set; }
        public string IP { get; set; }
        public string IPType { get; set; }
        public string Title { get; set; }

        [JsonIgnore]
        public bool IsDirty { get; set; }

        [JsonIgnore]
        public List<string> NewChildLinks { get; set; }

        public LinkData()
        {
            ChildLinks = new List<string>();
            BackLinks = new List<string>();
            NewChildLinks = new List<string>();
            
            IsDirty = false;
        }

        public LinkData Merge(LinkData data)
        {
            foreach (var cl in data.ChildLinks)
                if (!this.ChildLinks.Contains(cl))
                    this.ChildLinks.Add(cl);

            foreach (var bl in data.BackLinks)
                if (!this.BackLinks.Contains(bl))
                    this.BackLinks.Add(bl);

            this.Domain = data.Domain;
            this.DomainOrSubdomain = data.DomainOrSubdomain;
            this.DomainScheme = data.DomainScheme;
            this.Tld = data.Tld;
            this.IP = data.IP;
            this.IPType = data.IPType;
            this.Title = data.Title;
            this.IsDirty = true;

            return this;
        }
    }
}
