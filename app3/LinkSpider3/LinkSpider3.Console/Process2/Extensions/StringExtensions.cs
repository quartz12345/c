using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace LinkSpider3.Process2.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static UInt64 ToRabinFingerPrint(this string s)
        {
            if (s.IsNullOrEmpty())
                return 0;

            return Utils.RabinFingerPrint.ComputeFingerPrint(s);
        }

        public static string JsonSerialize(this object value)
        {
            //JavaScriptSerializer serializer = new JavaScriptSerializer();
            
            //StringBuilder json = new StringBuilder();
            //serializer.Serialize(value, json);

            //return json.ToString();
            return JsonConvert.SerializeObject(value, Formatting.None); 
        }

        public static string JsonSerialize(this object value, bool formatted)
        {
            return JsonConvert.SerializeObject(value, (formatted ? Formatting.Indented : Formatting.None));
        }

        public static T JsonDeserialize<T>(this string value)
        {
            //JavaScriptSerializer serializer = new JavaScriptSerializer();

            //T o;
            //try
            //{
            //    o = serializer.Deserialize<T>(value);
            //}
            //catch { o = Activator.CreateInstance<T>(); }

            //return o;

            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}
