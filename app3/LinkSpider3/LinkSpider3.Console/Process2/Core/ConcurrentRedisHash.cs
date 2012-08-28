using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using BookSleeve;
//using TeamDev.Redis;
//using TeamDev.Redis.LanguageItems;

using LinkSpider3.Process2.Extensions;

namespace LinkSpider3.Process2.Core
{
    public class ConcurrentRedisHash<T>
        : Core.DisposableBase
    {
        //private LanguageHash Hash;
        //private IHashCommands Hash;
        private RedisConnection Redis;
        private ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        private string Name;
        private int db;

        //public ConcurrentRedisHash(RedisDataAccessProvider redis, string name)
        public ConcurrentRedisHash(RedisConnection redis, string name, int db)
        {
            //Hash = redis.Hash[name];
            //Hash = redis.Hashes;
            Redis = redis;
            Name = name;
            this.db = db;
        }

        public T this[string field]
        {
            get
            {
                locker.EnterReadLock();

                try
                {
                    //string itemValue = Hash.Get(field);
                    var t = Redis.Hashes.GetString(db, Name, field);
                    string itemValue = Redis.Wait(t);
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
                    //Hash.Set(field, value.JsonSerialize());
                    var t = Redis.Hashes.Set(db, Name, field, value.JsonSerialize());
                    Redis.Wait(t);
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

        public long Count 
        { 
            get 
            { 
                //return Hash.Lenght; 
                var t = Redis.Hashes.GetLength(db, Name);
                return Redis.Wait(t);
            } 
        }

        public bool ContainsKey(string key)
        {
            locker.EnterReadLock();
            try
            {
                //return Hash.ContainsKey(key);
                var t = Redis.Hashes.Exists(db, Name, key);
                return Redis.Wait(t);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        //public KeyValuePair<string, T>[] ToArray()
        //{
        //    locker.EnterReadLock();
        //    try
        //    {
        //        KeyValuePair<string, T>[] newKVPs = new KeyValuePair<string, T>[Count];
        //        for (int i = 0; i < newKVPs.Length; i++)
        //        {
        //            KeyValuePair<string, T> newKVP = new KeyValuePair<string, T>(Hash.Keys[i], Hash.Values[i].JsonDeserialize<T>());
        //            newKVPs[i] = newKVP;
        //        }

        //        return newKVPs;
        //    }
        //    finally
        //    {
        //        locker.ExitReadLock();
        //    }
        //}

        protected override void Cleanup()
        {
            locker.Dispose();
        }
    }
}
