using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;

//using TeamDev.Redis;
using BookSleeve;

using LinkSpider3.Process2.Core;
using LinkSpider3.Process2.Data;
using LinkSpider3.Process2.Extensions;

namespace LinkSpider3.Process2.Persistence
{
    public class RedisPersistence
        : DisposableBase, IPersistence
    {
        IDictionary<string, string> properties;
        //RedisDataAccessProvider redis;
        RedisConnection redis;
        RedisTransaction trans;

        public RedisPersistence(IDictionary<string, string> properties)
        {
            this.properties = properties;
            //this.redis = new RedisDataAccessProvider();
            //this.redis.Configuration = new TeamDev.Redis.LanguageItems.Configuration();
            //this.redis.Configuration.Host = this.properties["server"];
            //this.redis.Configuration.Port = Convert.ToInt32(this.properties["port"]);
            this.redis = new RedisConnection(this.properties["server"], int.Parse(this.properties["port"]));
            this.redis.SetKeepAlive(1);
            this.redis.Open();
            trans = this.redis.CreateTransaction();
        }

        public object Load<T>(IDictionary<string, object> properties)
        {
            if (typeof(T).Equals(typeof(CollectorPool)))
            {
                //return new CollectorPool(this.redis.List["urn:pool"].Values);
                var t = this.redis.Lists.RangeString(0, "urn:pool", 0, -1);
                return new CollectorPool(this.redis.Wait(t));
            }

            //else if (typeof(T).Equals(typeof(ConcurrentRedisHash<LinkData>)))
            //{
            //    ConcurrentRedisHash<LinkData> links = new ConcurrentRedisHash<LinkData>(this.redis, "urn:link:data");

                //ConcurrentDictionary<string, LinkData> links =
            //    new ConcurrentDictionary<string, LinkData>(100, this.redis.Hash["urn:link:data"].Lenght);

                //Parallel.ForEach<string>(this.redis.Hash["urn:link:data"].Values,
            //    ld =>
            //    {
            //        LinkData ldObj = ld.JsonDeserialize<LinkData>();
            //        links.AddOrUpdate(ldObj.Link, ldObj,
            //            (link, oldLdObj) => 
            //            { 
            //                return oldLdObj; 
            //            });
            //    });

            //    return links;
            //}

            else
            {
                return Activator.CreateInstance(typeof(T), this.redis, properties["name"], properties["db"]);
            }
        }

        public void Save<T>(T o, IDictionary<string, object> properties)
        {
            throw new NotImplementedException();
        }

        protected override void Cleanup()
        {
            var t = this.trans.Execute();
            this.redis.Wait(t);

            this.trans.Dispose();
            this.redis.Dispose();
        }


        public bool Ping()
        {
            //TODO: Does not work if redis is not running, what's the use?
            //int result = this.redis.SendCommand(RedisCommand.PING);
            //return (result > 0);

            return true;
        }

        public object Provider
        {
            get { return this.redis; }
        }
    }
}
