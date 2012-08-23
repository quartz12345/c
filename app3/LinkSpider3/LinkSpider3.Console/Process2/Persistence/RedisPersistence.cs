using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using TeamDev.Redis;

using LinkSpider3.Process2.Core;
using LinkSpider3.Process2.Data;
using LinkSpider3.Process2.Extensions;

namespace LinkSpider3.Process2.Persistence
{
    public class RedisPersistence
        : DisposableBase, IPersistence
    {
        IDictionary<string, string> properties;
        RedisDataAccessProvider redis;

        public RedisPersistence(IDictionary<string, string> properties)
        {
            this.properties = properties;
            this.redis = new RedisDataAccessProvider();
            this.redis.Configuration = new TeamDev.Redis.LanguageItems.Configuration();
            this.redis.Configuration.Host = this.properties["server"];
            this.redis.Configuration.Port = Convert.ToInt32(this.properties["port"]);
        }

        public object Load<T>(IDictionary<string, object> properties)
        {
            if (typeof(T).Equals(typeof(CollectorPool)))
            {
                return new CollectorPool(this.redis.List["urn:pool"].Values);
            }
            else if (typeof(T).Equals(typeof(ConcurrentDictionary<string, LinkData>)))
            {
                ConcurrentDictionary<string, LinkData> links =
                    new ConcurrentDictionary<string, LinkData>(100, this.redis.Hash["urn:link:data"].Lenght);
                
                Parallel.ForEach<string>(this.redis.Hash["urn:link:data"].Values,
                    ld =>
                    {
                        LinkData ldObj = ld.JsonDeserialize<LinkData>();
                        links.AddOrUpdate(ldObj.Link, ldObj,
                            (link, oldLdObj) => 
                            { 
                                return oldLdObj; 
                            });
                    });
                
                return links;
            }

            return null;
        }

        public void Save<T>(T o, IDictionary<string, object> properties)
        {
            throw new NotImplementedException();
        }

        protected override void Cleanup()
        {
            this.redis.Close();
            this.redis.Dispose();
        }


        public bool Ping()
        {
            //TODO: Does not work if redis is not running, what's the use?
            //int result = this.redis.SendCommand(RedisCommand.PING);
            //return (result > 0);

            return true;
        }
    }
}
