using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml.Linq;

namespace LinkSpider3.Process
{
    public class LocationInfo
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public string Name { get; set; }
    }

    public class GeoLocationService
    {
        //public static LocationInfo GetLocationInfo()
        //{
        //    //TODO: How/where do we refactor this and tidy up the use of Context? This isn't testable.
        //    string ipaddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        //    LocationInfo v = new LocationInfo();

        //    if (ipaddress != "127.0.0.1")
        //        v = GeoLocationService.GetLocationInfo(ipaddress);
        //    else //debug locally
        //        v = new LocationInfo()
        //        {
        //            Name = "Sugar Grove, IL",
        //            CountryCode = "US",
        //            CountryName = "UNITED STATES",
        //            Latitude = 41.7696F,
        //            Longitude = -88.4588F
        //        };
        //    return v;
        //}

        private static Dictionary<string, LocationInfo> cachedIps = new Dictionary<string, LocationInfo>();

        public static LocationInfo GetLocationInfo(string ipParam)
        {
            LocationInfo result = null;
            IPAddress i = System.Net.IPAddress.Parse(ipParam);
            string ip = i.ToString();
            if (!cachedIps.ContainsKey(ip))
            {
                string r;
                using (var w = new WebClient())
                {
                    r = w.DownloadString(String.Format("http://api.hostip.info/?ip={0}&position=true", ip));
                }

                /*
             string r =
                @"<?xml version=""1.0"" encoding=""ISO-8859-1"" ?>
<HostipLookupResultSet version=""1.0.0"" xmlns=""http://www.hostip.info/api"" xmlns:gml=""http://www.opengis.net/gml"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.hostip.info/api/hostip-1.0.0.xsd"">
 <gml:description>This is the Hostip Lookup Service</gml:description>
 <gml:name>hostip</gml:name>
 <gml:boundedBy>
    <gml:Null>inapplicable</gml:Null>
 </gml:boundedBy>
 <gml:featureMember>
    <Hostip>
     <gml:name>Sugar Grove, IL</gml:name>
     <countryName>UNITED STATES</countryName>
     <countryAbbrev>US</countryAbbrev>
     <!-- Co-ordinates are available as lng,lat -->
     <ipLocation>
        <gml:PointProperty>
         <gml:Point srsName=""http://www.opengis.net/gml/srs/epsg.xml#4326"">
            <gml:coordinates>-88.4588,41.7696</gml:coordinates>
         </gml:Point>
        </gml:PointProperty>
     </ipLocation>
    </Hostip>
 </gml:featureMember>
</HostipLookupResultSet>";
             */

                var xmlResponse = XDocument.Parse(r);
                var gml = (XNamespace)"http://www.opengis.net/gml";
                //var ns = (XNamespace)"http://www.hostip.info/api";
                var ns = string.Empty;

                try
                {
                    result = (from x in xmlResponse.Descendants(ns + "Hostip")
                              select new LocationInfo
                              {
                                  CountryCode = x.Element(ns + "countryAbbrev").Value,
                                  CountryName = x.Element(ns + "countryName").Value,
                                  //Latitude = float.Parse(x.Descendants(gml + "coordinates").Single().Value.Split(',')[0]),
                                  //Longitude = float.Parse(x.Descendants(gml + "coordinates").Single().Value.Split(',')[1]),
                                  Name = x.Element(gml + "name").Value
                              }).SingleOrDefault();
                }
                catch (NullReferenceException)
                {
                    //Looks like we didn't get what we expected.
                }
                if (result != null)
                {
                    cachedIps.Add(ip, result);
                }
            }
            else
            {
                result = cachedIps[ip];
            }
            return result;
        }
    }

}
