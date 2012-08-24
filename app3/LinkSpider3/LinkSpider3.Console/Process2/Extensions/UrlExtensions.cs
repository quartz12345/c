using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace LinkSpider3.Process2.Extensions
{
    public static class UrlExtensions
    {
        public static string NormalizeUri(this string url, string baseUrl)
        {
            if (url.IsNullOrEmpty())
            {
                return baseUrl;
            }

            // Escaped the url before checking
            url = Uri.EscapeUriString(url);

            if (Uri.IsWellFormedUriString(url, UriKind.Relative))
            {
                if (!baseUrl.IsNullOrEmpty())
                {
                    Uri absoluteBaseUrl = new Uri(baseUrl, UriKind.Absolute);
                    return new Uri(absoluteBaseUrl, url).ToString();
                }

                Uri uri = new Uri(url, UriKind.Relative);
                
                if (uri.OriginalString == "//")
                    return null;
                
                return uri.ToString();
            }

            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                // Only handle same schema as base uri
                Uri uri = new Uri(url);

                if (Uri.CheckSchemeName(uri.Scheme) &&
                    uri.Scheme.ToLowerInvariant() != "javascript" &&
                    uri.Scheme.ToLowerInvariant() != "mailto" &&
                    uri.Fragment.IsNullOrEmpty() &&
                    Uri.CheckHostName(uri.Host) == UriHostNameType.Dns)
                {
                    if (baseUrl.IsNullOrEmpty())
                    {
                        return uri.ToString();
                    }
                    else
                    {
                        Uri baseUri = new Uri(baseUrl);

                        bool schemaMatch;

                        // Special case for http/https
                        if ((baseUri.Scheme == Uri.UriSchemeHttp) ||
                            (baseUri.Scheme == Uri.UriSchemeHttps))
                        {
                            schemaMatch = string.Compare(Uri.UriSchemeHttp, uri.Scheme, StringComparison.OrdinalIgnoreCase) == 0 ||
                                string.Compare(Uri.UriSchemeHttps, uri.Scheme, StringComparison.OrdinalIgnoreCase) == 0;
                        }
                        else
                        {
                            schemaMatch = string.Compare(baseUri.Scheme, uri.Scheme, StringComparison.OrdinalIgnoreCase) == 0;
                        }

                        if (schemaMatch)
                        {
                            return new Uri(url, UriKind.Absolute).ToString();
                        }
                    }
                }
            }

            return null;
        }

        public static Uri ToAuthorityUri(this Uri uri)
        {
            return new Uri(uri.GetLeftPart(UriPartial.Authority));
        }

        public static Uri ToUri(this string url)
        {
            string verifiedUrl = url.NormalizeUri(string.Empty);
            if (verifiedUrl.IsNullOrEmpty())
                return null;
            else
            {
                if (url.StartsWith("http://www.") ||
                    url.StartsWith("https://www.") ||
                    url.StartsWith("http://") ||
                    url.StartsWith("https://"))
                {
                    return new Uri(url);
                }

                UriBuilder b = new UriBuilder(url);
                b.Scheme = Uri.UriSchemeHttp;

                if (Uri.CheckHostName(b.Host) == UriHostNameType.Dns)
                {
                    if (!b.Host.StartsWith("www."))
                        b.Host = "www." + b.Host;
                }

                return b.Uri;
            }
        }

        public static string ToUriShort(this Uri uri)
        {
            //StringBuilder s = new StringBuilder();
            
            //if (uri.Host.Length > 15)
            //    s.Append(uri.Host.Substring(0, 15));
            //else
            //    s.Append(uri.Host);

            //s.Append("~");

            //if (uri.ToString().Length > 10)
            //    s.Append(uri.ToString().Substring(uri.ToString().Length - 10));
            
            //return s.ToString();

            StringBuilder s = new StringBuilder(uri.ToString());

            if (s.Length > 30)
                return s.ToString(0, 30) + "...";
            else
                return s.ToString();
        }
    }
}
