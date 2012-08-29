using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Driver;

using LinkSpider3.Process2.Core;
using LinkSpider3.Process2.Extensions;

namespace LinkSpider3.Process2.Persistence
{
    public class MongoPersistence
        : DisposableBase, IPersistence
    {
        MongoServer server;
        MongoDatabase database;

        public MongoPersistence(IDictionary<string, string> properties)
        {
            string connectionString = string.Format("mongodb://{0}/?safe=true", properties["server"]);
            this.server = MongoServer.Create(connectionString);
            this.database = this.server.GetDatabase(properties["database"]);
        }

        protected override void Cleanup()
        {
            //
        }

        public object Provider
        {
            get 
            {
                return this.server;
            }
        }

        public object Load<T>(IDictionary<string, object> properties)
        {
            if (typeof(T).Equals(typeof(CollectorPool)))
            {
                var collection = this.database.GetCollection<string>("urn:pool");
                return new CollectorPool(collection.FindAll());
            }
            else
            {
                return Activator.CreateInstance(typeof(T), this.database, properties["name"]);
            }
        }

        public void Save<T>(T o, IDictionary<string, object> properties)
        {
            if (typeof(T).Equals(typeof(CollectorPool)))
            {
                CollectorPool pool = o as CollectorPool;
                var collection = this.database.GetCollection<string>("urn:pool");
                collection.RemoveAll();
                collection.InsertBatch(pool.ToArray());
                return;
            }

            throw new NotImplementedException();
        }

        public bool Ping()
        {
            try
            {
                this.server.Ping();
                return true;
            }
            catch { return false; }
        }


        public string ProviderName
        {
            get { return "mongodb"; }
        }
    }
}
