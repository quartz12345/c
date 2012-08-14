using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LinkSpider3.Process
{
    public class LinkParser
    {
        static internal string Validate(string link, string backlink)
        {
            if (!string.IsNullOrEmpty(link))
            {
                if (Regex.IsMatch(link, @"^\w+://"))
                {
                    link = new Uri(link).AbsoluteUri;
                }
                else
                {
                    if (string.IsNullOrEmpty(backlink))
                    {
                        //Uri possibleUri;
                        //if (Uri.TryCreate(link, UriKind.Relative, out possibleUri))
                        //{
                        //    link = possibleUri.AbsoluteUri;
                        //}
                        //else
                        //{
                        //    link = string.Empty;
                        //}

                        // Unparsable link
                        link = string.Empty;
                    }
                    else
                    {
                        // Possibly a relative uri, try combining it with the backlink
                        Uri possibleUri;
                        if (Uri.TryCreate(new Uri(backlink), link, out possibleUri))
                        {
                            link = possibleUri.AbsoluteUri;
                            if (link.StartsWith("javascript:"))
                                link = string.Empty;
                            if(link.Equals("#"))
                                link = string.Empty;
                        }
                        else
                        {
                            link = string.Empty;
                        }
                    }
                }
            }

            return link;
        }
    }
}
