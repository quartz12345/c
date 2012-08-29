using System;

namespace LinkSpider3.Process2.Core
{
    public interface IConcurrentHash<T>
    {
        T AddOrUpdate(string key, T newValue, Func<string, T, T> update);
        bool ContainsKey(string key);
        long Count { get; }
        T this[string field] { get; set; }
    }
}
