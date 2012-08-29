using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkSpider3.Process2.Data
{
    public class SimpleObject<TValue>
    {
        public string Id { get; set; }
        public TValue Value { get; set; }
    }
}
