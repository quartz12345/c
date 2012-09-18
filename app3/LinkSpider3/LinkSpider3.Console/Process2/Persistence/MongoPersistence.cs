using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Driver;
using MongoDB.Bson;

using LinkSpider3.Process2.Core;
using LinkSpider3.Process2.Extensions;
using LinkSpider3.Process2.Data;

namespace LinkSpider3.Process2.Persistence
{
    public class MongoPersistence
        : DisposableBase, IPersistence
    {
        MongoServer server;
        MongoDatabase database;
        //MongoCollection<BsonDocument> collection;

        public MongoPersistence(IDictionary<string, string> properties)
        {
            //This will check if username and password on Program.cs for mongoDB: TODO
            string connectionString = "";
           // if (properties["user"] != "" && properties["pass"] != "")
            //{
               // connectionString = string.Format("mongodb://{0}:{1}@{2}/?safe=true", new object[] {properties["user"], properties["pass"], properties["server"]});
            //}
            //else
            //{
               // connectionString = string.Format("mongodb://{0}/?safe=true", properties["server"]);
            //}

            connectionString = "mongodb://madmin:mpass@50.62.1.71/?safe=true";
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
                var collection = this.database.GetCollection<SimpleObject<List<string>>>("urn:pool");
                var so = collection.FindOne();
                if (so.IsNull())
                    return new CollectorPool();

                return new CollectorPool(so.Value);
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
                var collection = this.database.GetCollection<SimpleObject<List<string>>>("urn:pool");
                var so = collection.FindOne();
                if (so.IsNull())
                {
                    so = new SimpleObject<List<string>>();
                    so.Id = "1";
                }
                so.Value = new List<string>(pool.ToArray());
                collection.Save(so);
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
        /*
        public bool isToCrawl(string Url)
        {
            bool crawl = false;
            String urlCRC32 = Url.ToHashString();
            String dateToday = DateTime.Today.ToString("yyMMdd");
            var query = new QueryDocument {
                { "_id", urlCRC32 },
                { "Date", dateToday }
            };
            this.collection = this.database.GetCollection("urn:link:crawldate:current");
            if (this.collection.Find(query).Count() == 0)
            {
                crawl = true;
            }
            return crawl;
        }
         * */
    }
}
