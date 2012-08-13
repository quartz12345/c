using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using System.Web.Script.Serialization;
using System.IO;
using System.Text.RegularExpressions;

namespace LinkSpider3
{
    public static class GlobalExtensions
    {
        public static string GetSHA1Code(this string value)
        {
            using (SHA1 sha1 = new SHA1CryptoServiceProvider())
            {
                return Convert.ToBase64String(
                    sha1.ComputeHash(Encoding.UTF8.GetBytes(value)));
            }
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
