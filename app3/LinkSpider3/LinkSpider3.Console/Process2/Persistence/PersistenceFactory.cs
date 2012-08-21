using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkSpider3.Process2.Persistence
{
    public static class PersistenceFactory
    {
        public static IPersistence GetPersistence(
            string provider, IDictionary<string, string> properties)
        {
            if (provider == "redis")
                return Activator.CreateInstance(typeof(RedisPersistence), properties) as IPersistence;

            throw new NotSupportedException("Provider `" + provider + "` is not supported.");
        }
    }
}
