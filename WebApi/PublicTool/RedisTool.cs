using StackExchange.Redis;
using System;

namespace PublicTool
{
    /// <summary>
    /// Redis帮助类
    /// </summary>
    public class RedisTool : IDisposable
    {

        //https://blog.csdn.net/zyt986710/article/details/124985068  安装教程
        private static readonly object Locker = new object();
        private static ConnectionMultiplexer redis = null;
        private static bool connected = false;
        private IDatabase db = null;
        private static string _ip;
        private static string _port;
        private static string _pwd;

        //public bool IsConnected { get { GetRedisConnect(); return redis.IsConnected; } }

        public RedisTool()
        {
            _ip = "127.0.0.1";
            _port = "6379";
            //_pwd = pwd;
            //加载连接
            GetRedisConnect();
        }

        /// <summary>
        /// Redis连接
        /// </summary>
        /// <returns></returns>
        private int GetRedisConnect()
        {
            if (connected)
            {
                return 0;
            }
            lock (Locker)
            {
                if (redis == null || !redis.IsConnected)
                {
                    redis = ConnectionMultiplexer.Connect($"{_ip}:{_port},password={_pwd},abortConnect = false");
                }
            }
            connected = true;
            return 0;
        }

        public void Using(Action<RedisTool> action)
        {
            using (var redisHelper = new RedisTool())
            {
                action(redisHelper);
            }
        }

        public RedisTool Use(int i = 0)
        {
            db = redis.GetDatabase(i);
            //Log.Logs($"RedisDB Conntet State: {redis.IsConnected}");
            var t = db.Ping();
            //Log.Logs($"RedisDB Select {i}, Ping.{t.TotalMilliseconds}ms");
            return this;
        }

        public void Close(bool allowCommandsToComplete = true)
        {
            if (redis != null)
            {
                redis.Close(allowCommandsToComplete);
            }
        }

        public void CloseAsync(bool allowCommandsToComplete = true)
        {
            if (redis != null)
            {
                redis.CloseAsync(allowCommandsToComplete);
            }
        }

        public void Dispose()
        {
            db = null;
        }

        #region 发布订阅

        public delegate void RedisDeletegate(string str);
        public event RedisDeletegate RedisSubMessageEvent;

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public long RedisPublish(string channel, string msg)
        {
            ISubscriber sub = redis.GetSubscriber();
            return sub.Publish(channel, msg);
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="subChannael"></param>
        /// <param name=""></param>
        public void RedisSubScribe(string subChannael)
        {
            ISubscriber sub = redis.GetSubscriber();
            sub.Subscribe(subChannael, (channel, message) =>
            {
                RedisSubMessageEvent?.Invoke(message); //触发事件
            });
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="channel"></param>
        public void Unsubscribe(string channel)
        {
            ISubscriber sub = redis.GetSubscriber();
            sub.Unsubscribe(channel);
        }

        /// <summary>
        /// 取消全部订阅
        /// </summary>
        public void UnsubscribeAll()
        {
            ISubscriber sub = redis.GetSubscriber();
            sub.UnsubscribeAll();
        }

        #endregion

        #region String类型操作

        /// <summary>
        ///  设置指定key 的值(默认第0个库)
        /// </summary>
        /// <param name="strKey">指定key的值</param>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public bool InsertStr(string strKey, string strValue)
        {
            var db = redis.GetDatabase();
            return db.StringSet(strKey, strValue);
        }

        /// <summary>
        ///  设置指定key 的值(指定库)
        /// </summary>
        /// <param name="strKey"></param>
        /// <param name="strValue"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public bool InsertStr(string strKey, string strValue, int database)
        {
            var db = redis.GetDatabase(database);
            return db.StringSet(strKey, strValue);
        }

        /// <summary>
        ///  设置指定key 的值(指定第一个库,指定过期时间)
        /// </summary>
        /// <param name="strKey">指定key的值</param>
        /// <param name="strValue"></param>
        /// <param name="expire"></param>
        public bool InsertStrExpire(string strKey, string strValue, DateTime expire)
        {
            var db = redis.GetDatabase(1);
            db.StringSet(strKey, strValue);
            return db.KeyExpire(strKey, expire);
        }

        /// <summary>
        ///   设置指定key 的值(指定第一个库,指定过期分钟数)
        /// </summary>
        /// <param name="strKey">指定key的值</param>
        /// <param name="strValue"></param>
        /// <param name="timespanmin"></param>
        public bool InsertStrExpire(string strKey, string strValue, int timespanmin)
        {
            var db = redis.GetDatabase(1);
            return db.StringSet(strKey, strValue, TimeSpan.FromMinutes(timespanmin));
        }

        /// <summary>
        ///   设置指定key 的值(指定第一个库,指定过期分钟数)
        /// </summary>
        /// <param name="strKey">指定key的值</param>
        /// <param name="strValue"></param>
        /// <param name="timespanmin"></param>
        public void InsertStrExpireByDatabaseOne(string strKey, string strValue, int timespanmin)
        {
            var db = redis.GetDatabase(1);
            db.StringSet(strKey, strValue, TimeSpan.FromMinutes(timespanmin));
        }

        /// <summary>
        ///   获取指定 key 的值(默认第0个库)
        /// </summary>
        /// <param name="strKey">指定key的值</param>
        /// <returns></returns>
        public string ReadStr(string strKey)
        {
            var db = redis.GetDatabase();
            return db.StringGet(strKey);
        }

        /// <summary>
        /// 获取指定 key 的值(指定第一个库)
        /// </summary>
        /// <param name="strKey">指定key的值</param>
        /// <returns></returns>
        public string ReadStrByDatabaseOne(string strKey)
        {
            var db = redis.GetDatabase(1);
            return db.StringGet(strKey);
        }

        /// <summary>
        /// 获取指定 key 的值(指定第一个库)
        /// </summary>
        /// <param name="strKey">指定key的值</param>
        /// <param name="database"></param>
        /// <returns></returns>
        public string ReadStrByDatabase(string strKey, int database)
        {
            var db = redis.GetDatabase(database);
            return db.StringGet(strKey);
        }

        /// <summary>
        /// 删除指定key的值(默认第0个库)
        /// </summary>
        /// <param name="strKey">指定key的值</param>
        public bool DeleteStr(string strKey)
        {
            var db = redis.GetDatabase();
            return db.KeyDelete(strKey);
        }

        /// <summary>
        /// 删除指定key的值(默认第0个库)
        /// </summary>
        /// <param name="strKey">指定key的值</param>
        /// <param name="database">指定库的值</param>
        public bool DeleteStrByDatabase(string strKey, int database)
        {
            var db = redis.GetDatabase(database);
            return db.KeyDelete(strKey);
        }

        public bool Exist(string strKey)
        {
            var db = redis.GetDatabase();
            return db.KeyExists(strKey);
        }

        #endregion

        #region Hash类型操作

        public bool InsertHash(string tablename, string strKey, string strValue)
        {
            var db = redis.GetDatabase();
            return db.HashSet(tablename, strKey, strValue);
        }

        public string ReadHash(string tablename, string strKey)
        {
            var db = redis.GetDatabase();
            return db.HashGet(tablename, strKey);
        }

        public bool ExistHash(string tablename, string strKey)
        {
            var db = redis.GetDatabase();
            return db.HashExists(tablename, strKey);
        }

        public bool DeleteHash(string tablename, string strKey)
        {
            var db = redis.GetDatabase();
            return db.HashDelete(tablename, strKey);
        }

        #endregion
    }
}
