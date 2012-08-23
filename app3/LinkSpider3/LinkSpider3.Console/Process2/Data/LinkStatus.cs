using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkSpider3.Process2.Data
{
    public class LinkStatus
    {
        public string Link { get; set; }
        public int Status { get; set; }

        // yyMMdd
        public string Date { get; set; }

        // HHmmss
        public string Time { get; set; }
    }
}
