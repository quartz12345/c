using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkSpider3.Process2.Data
{
    public class AnchorData
    {
        public string Text { get; set; }
        public string Rel { get; set; }
        public string Kind { get; set; }

        public override string ToString()
        {
            return string.Concat(Text, ":", Rel, ":", Kind);
        }
    }
}
