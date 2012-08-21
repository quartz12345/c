using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

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
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            
            StringBuilder json = new StringBuilder();
            serializer.Serialize(value, json);

            return json.ToString();
        }

        public static T JsonDeserialize<T>(this string value)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            T o;
            try
            {
                o = serializer.Deserialize<T>(value);
            }
            catch { o = Activator.CreateInstance<T>(); }

            return o;
        }
    }
}
