using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkSpider3.Process2.Persistence
{
    public interface IPersistence
    {
        object Provider { get; }
        string ProviderName { get; }
        object Load<T>(IDictionary<string, object> properties);
        void Save<T>(T o, IDictionary<string, object> properties);
        bool Ping();
    }
}
