using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkSpider3.Process2.Data
{
    public class LinkData
    {
        public List<string> ChildLinks { get; set; }
        public List<string> BackLinks { get; set; }
        public string Link { get; set; }

        public LinkData()
        {
            ChildLinks = new List<string>();
            BackLinks = new List<string>();
        }
    }
}
