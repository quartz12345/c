using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using TeamDev.Redis;
using TeamDev.Redis.LanguageItems;

using LinkSpider3.Process2.Extensions;

namespace LinkSpider3.Process2.Core
{
    public class ConcurrentRedisHash<T>
        : Core.DisposableBase
    {
        private LanguageHash Hash;
        private ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        public ConcurrentRedisHash(RedisDataAccessProvider redis, string name)
        {
            Hash = redis.Hash[name];
        }

        public T this[string field]
        {
            get
            {
                locker.EnterReadLock();

                try
                {
                    string itemValue = Hash.Get(field);
                    if (itemValue.IsNullOrEmpty())
                    {
                        return Activator.CreateInstance<T>();
                    }
                    else
                    {
                        return itemValue.JsonDeserialize<T>();
                    }
                }
                finally
                {
                    locker.ExitReadLock();
                }
            }

            set
            {
                locker.EnterWriteLock();

                try
                {
                    Hash.Set(field, value.JsonSerialize());
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
        }


        public T AddOrUpdate(string key, T newValue, Func<string, T, T> update)
        {
            if (ContainsKey(key))
            {
                T o = update(key, this[key]);
                this[key] = o;
                return o;
            }
            else
            {
                this[key] = newValue;
                return newValue;
            }
        }

        public int Count { get { return Hash.Lenght; } }

        public bool ContainsKey(string key)
        {
            locker.EnterReadLock();
            try
            {
                return Hash.ContainsKey(key);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public KeyValuePair<string, T>[] ToArray()
        {
            locker.EnterReadLock();
            try
            {
                KeyValuePair<string, T>[] newKVPs = new KeyValuePair<string, T>[Hash.Lenght];
                for (int i = 0; i < newKVPs.Length; i++)
                {
                    KeyValuePair<string, T> newKVP = new KeyValuePair<string, T>(Hash.Keys[i], Hash.Values[i].JsonDeserialize<T>());
                    newKVPs[i] = newKVP;
                }

                return newKVPs;
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        protected override void Cleanup()
        {
            locker.Dispose();
        }
    }
}
