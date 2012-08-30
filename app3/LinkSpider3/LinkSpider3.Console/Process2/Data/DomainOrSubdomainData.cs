using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkSpider3.Process2.Data
{
    public class DomainOrSubdomainData
    {
        public string DomainOrSubdomain { get; set; }
        public List<string> Links { get; set; }
    }
}
