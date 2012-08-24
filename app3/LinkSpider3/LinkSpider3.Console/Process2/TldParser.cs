using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using LinkSpider3.Process2.Extensions;

namespace LinkSpider3.Process2
{
    public class TldParser
    {
        private List<string> TldMozillaList = new List<string>();

        public TldParser()
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

        public string GetTld(string host)
        {
            string tld = 
                TldMozillaList
                .Where(t => host.EndsWith(t))
                .FirstOrDefault();

            if (tld.IsNullOrEmpty())
            {
                // Excluded in the TldMozillaList
                if (host.EndsWith(".co.uk"))
                    return "co.uk";
                if (host.EndsWith(".com.ve"))
                    return "com.ve";
            }
            else
            {
                // Ridiculous occurance
                if (tld == host)
                    return string.Empty;

                return tld;
            }

            return string.Empty;
        }
    }
}
