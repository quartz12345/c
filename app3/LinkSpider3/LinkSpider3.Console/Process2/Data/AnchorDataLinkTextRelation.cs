using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkSpider3.Process2.Data
{
    public class AnchorDataLinkTextRelation
    {
        // Concatenation of Link & ChildLink RF_ID
        public string LinkChildLinkRelation_RFID { get; set; }

        public string AnchorText { get; set; }

        public string Link { get; set; }

        public string ChildLink { get; set; }

        public override string ToString()
        {
            return string.Format("{0} : {1} : {2}",
                AnchorText, Link, ChildLink);
        }
    }
}
