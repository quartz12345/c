using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

using LinkSpider3.Process2.Extensions;

namespace LinkSpider3.Process2.Core
{
    public class ConcurrentMongoHash<T>
        : DisposableBase, IConcurrentHash<T>
    {
        private ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        private MongoDatabase database;
        private string name;
        private MongoCollection collection;

        public ConcurrentMongoHash(MongoDatabase database, string name)
        {
            this.database = database;
            this.name = name;
            this.collection = this.database.GetCollection(this.name);
        }

        protected override void Cleanup()
        {
            locker.Dispose();
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

        public bool ContainsKey(string field)
        {
            return collection.Count(Query.EQ("_id", field)) > 0;
        }

        public long Count
        {
            get 
            {
                return collection.Count();
            }
        }

        public T this[string field]
        {
            get
            {
                var q = Query.EQ("_id", field);
                var v = collection.FindOneAs<T>(q);
                if (v.IsNull())
                {
                    return Activator.CreateInstance<T>();
                }
                else
                {
                    return v;
                }
            }
            set
            {
                collection.Save(value);
            }
        }
    }
}
