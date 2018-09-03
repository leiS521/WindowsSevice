using ProtoBuf;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfficeService
{
	public class RedisManager
	{
		#region init
		private IDatabase db;
		private ISubscriber sub;
		private RedisConnectionStringElement config;
		private IServer server;
		private void Init(string configName)
		{
			//@"Host"
			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(GetConnectionString(configName));
			db = redis.GetDatabase(config.DB);
			sub = redis.GetSubscriber();
			server = redis.GetServer(config.ConnectionString, config.Port);
		}

		/// <summary>
		/// 获取连接字符串
		/// </summary>
		/// <param name="configName"></param>
		/// <returns></returns>
		private string GetConnectionString(string configName)
		{
			var a = ((YuanXinRedisConfigSettings)ConfigurationManager.GetSection("yuanxinRedisSettings")).ConnectionOptions[configName];
			config = YuanXinRedisConfigSettings.GetConfig().ConnectionOptions[configName];
			StringBuilder conectionString = new StringBuilder();
			conectionString.AppendFormat(@"{0}:{1}", config.ConnectionString, config.Port);
			if (!string.IsNullOrEmpty(config.PassWord))
			{
				conectionString.AppendFormat(@",password={0},ConnectTimeout=10000,abortConnect=false", config.PassWord);
			}
			return conectionString.ToString();
		}

		/// <summary>
		/// 将配置文件中的连接字符串传入
		/// </summary>
		/// <param name="configName"></param>
		public RedisManager(string configName)
		{
			Init(configName);
		}
		#endregion

		/// <summary>
		/// 序列化
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <returns></returns>
		public RedisValue Serialize<T>(T data)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Serializer.Serialize(ms, data);
				return ms.ToArray();
			}
		}

		/// <summary>
		/// 反序列化
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public T DeSerialize<T>(RedisValue value)
		{
			using (MemoryStream ms = new MemoryStream(value))
			{
				return Serializer.Deserialize<T>(ms);
			}
		}

		/// <summary>
		/// Save String Data
		/// </summary>
		/// <param name="keys"></param>
		/// <param name="values"></param>
		/// <param name="expiredTime"></param>
		/// <returns></returns>
		public Task<bool> SaveAsync<T>(string key, T value, TimeSpan expiredTime)
		{

			RedisValue rValue = this.Serialize(value);
			var tran = db.CreateTransaction(true);
			tran.StringSetAsync(key, rValue, expiredTime);
			return tran.ExecuteAsync();
		}

		/// <summary>
		/// Get String Data
		/// </summary>
		/// <param name="keys"></param>
		/// <param name="values"></param>
		/// <param name="expiredTime"></param>
		/// <returns></returns>
		public Task<T> GetAsync<T>(string key)
		{
			var redisValue = db.StringGetAsync(key);
			if (!redisValue.Result.IsNullOrEmpty)
				return Task.FromResult(this.DeSerialize<T>(redisValue.Result));
			else
				return null;
		}

		/// <summary>
		/// Save String Data
		/// </summary>
		/// <param name="keys"></param>
		/// <param name="values"></param>
		/// <param name="expiredTime"></param>
		/// <returns></returns>
		public Task<bool> StringSetAsync(RedisKey[] keys, RedisValue[] values, TimeSpan[] expiredTime)
		{
			var tran = db.CreateTransaction(true);
			for (var i = 0; i < keys.Length; i++)
			{
				var key = keys[i];
				var val = values[i];
				var expTime = expiredTime[i];
				if (expTime == TimeSpan.Zero)
				{
					tran.StringSetAsync(key, val);
				}
				else
				{
					tran.StringSetAsync(key, val, expTime);
				}
			}
			return tran.ExecuteAsync();
		}

		/// <summary>
		///Get String Data
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public Task<RedisValue> StringGetAsync(RedisKey key)
		{
			return db.StringGetAsync(key);
		}

		/// <summary>
		/// 检测key是否过期
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public Task<bool> KeyExistsAsync(RedisKey key)
		{
			return db.KeyExistsAsync(key);
		}

		/// <summary>
		/// 设置key过期时间
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public Task<bool> KeyTimeToLiveAsync(RedisKey key)
		{
			TimeSpan? keyTime = db.KeyTimeToLiveAsync(key).Result;
			return keyTime.HasValue ? Task.FromResult(keyTime.Value.Seconds > 0) : Task.FromResult(false);
		}

		/// <summary>
		/// 删除指定key
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		public Task<bool> DeleteKeyAsync(RedisKey[] keys)
		{
			var tran = db.CreateTransaction(true);
			foreach (var key in keys)
			{
				tran.KeyDeleteAsync(key);
			}
			return tran.ExecuteAsync();
		}

		/// <summary>
		/// 添加项到list
		/// </summary>
		/// <param name="keys"></param>
		/// <param name="values"></param>
		public Task<bool> AddItemToListAsync(RedisKey[] keys, RedisValue[] values)
		{
			var tran = db.CreateTransaction(true);
			for (int i = 0; i < keys.Length; i++)
			{
				var key = keys[i];
				var val = values[i];
				tran.ListRightPushAsync(key, val);
			}
			return tran.ExecuteAsync();
		}

		/// <summary>
		/// 设置key过期时间
		/// </summary>
		/// <param name="key"></param>
		/// <param name="expiredTime"></param>
		/// <returns></returns>
		public Task<bool> KeyExpireAsync(RedisKey key, TimeSpan expiredTime)
		{
			return db.KeyExpireAsync(key, expiredTime);
		}

		/// <summary>
		/// 取list里的项,(项还是存在只是取值)
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public Task<RedisValue[]> GetAllItemsFromListAsync(RedisKey key)
		{
			return db.ListRangeAsync(key);
		}

		/// <summary>
		///  取list里的项
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public Task<RedisValue> ListLeftPopAsync(RedisKey key)
		{
			return db.ListLeftPopAsync(key);
		}

		/// <summary>
		/// 查看list长度
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public Task<long> ListLengthAsync(RedisKey key)
		{
			return db.ListLengthAsync(key);
		}

		/// <summary>
		/// Save HashSet Data
		/// </summary>
		/// <param name="key"></param>
		/// <param name="fields"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		public Task<bool> HashSetAsync(RedisKey key, RedisValue[] fields, RedisValue[] values)
		{
			var tran = db.CreateTransaction(true);
			for (var i = 0; i < fields.Length; i++)
			{
				tran.HashSetAsync(key, fields[i], values[i]);
			}
			return tran.ExecuteAsync();
		}

		/// <summary>
		/// Save HashSet Data
		/// </summary>
		/// <param name="key"></param>
		/// <param name="fields"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		public Task<bool> HashSetAsync(RedisKey key, RedisValue fields, RedisValue values)
		{
			var tran = db.CreateTransaction(true);
			tran.HashSetAsync(key, fields, values);
			return tran.ExecuteAsync();
		}

		/// <summary>
		/// 向指定键的字段，增加指定值
		/// </summary>
		/// <param name="key"></param>
		/// <param name="fields"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		public Task<bool> HashIncrementAsync(RedisKey key, RedisValue fields, double values)
		{
			var tran = db.CreateTransaction(true);
			tran.HashIncrementAsync(key, fields, values);
			return tran.ExecuteAsync();
		}

		/// <summary>
		/// HashGetAsync
		/// </summary>
		/// <param name="key"></param>
		/// <param name="fields"></param>
		/// <returns></returns>
		public Task<RedisValue> HashGetAsync(RedisKey key, RedisValue fields)
		{
			return db.HashGetAsync(key, fields);
		}

		/// <summary>
		/// HashGetAsync
		/// </summary>
		/// <param name="key"></param>
		/// <param name="fields"></param>
		/// <returns></returns>
		public Task<RedisValue[]> HashGetAsync(RedisKey key, RedisValue[] fields)
		{
			return db.HashGetAsync(key, fields);
		}

		/// <summary>
		/// HashGetAllAsync
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public Task<HashEntry[]> HashGetAllAsync(RedisKey key)
		{
			return db.HashGetAllAsync(key);
		}

		/// <summary>
		/// GetAllKeys
		/// </summary>
		/// <returns></returns>
		public List<RedisKey> GetAllKeys()
		{
			return server.Keys(config.DB).ToList();
		}

		/// <summary>
		/// redis发布消息到指定频道
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="messgae"></param>
		/// <returns></returns>
		public Task<bool> PublishAsync(RedisChannel channel, RedisValue[] messgae)
		{
			var tran = db.CreateTransaction(true);
			foreach (var item in messgae)
			{
				tran.PublishAsync(channel, item);
			}
			return tran.ExecuteAsync();
		}

		/// <summary>
		/// 无序排列存储
		/// </summary>
		/// <param name="key">RedisKey</param>
		/// <param name="values">RedisValue</param>
		/// <returns></returns>
		public bool SetAdd(RedisKey key, RedisValue values)
		{
			return db.SetAdd(key, values);
		}

		/// <summary>
		/// 无序列表获取数据方法法
		/// </summary>
		/// <param name="setOperation">并集，交集，差集</param>
		/// <param name="first">firstKey</param>
		/// <param name="second">secondKey</param>
		/// <returns></returns>
		public RedisValue[] SetCombine(SetOperation setOperation, RedisKey first, RedisKey second)
		{
			return db.SetCombine(setOperation, first, second);
		}

		/// <summary>
		/// 删除指定value
		/// </summary>
		/// <returns></returns>
		public bool SetRemove(RedisKey key, RedisValue value)
		{
			return db.SetRemove(key, value);
		}
	}

	public class YuanXinRedisConfigSettings : ConfigurationSection
	{
		public static YuanXinRedisConfigSettings GetConfig()
		{
			YuanXinRedisConfigSettings result = (YuanXinRedisConfigSettings)ConfigurationManager.GetSection("yuanxinRedisSettings");

			if (result == null)
				result = new YuanXinRedisConfigSettings();

			return result;
		}

		[ConfigurationProperty("connectionOptions")]
		public RedisConnectionStringElementCollection ConnectionOptions
		{
			get
			{
				return (RedisConnectionStringElementCollection)base["connectionOptions"];
			}
		}
	}

	public class RedisConnectionStringElementCollection : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new RedisConnectionStringElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((RedisConnectionStringElement)element).Name;
		}

		public RedisConnectionStringElement this[string name]
		{
			get
			{
				return BaseGet(name) as RedisConnectionStringElement;
			}
		}
	}

	public class RedisConnectionStringElement : ConfigurationElement
	{
		[ConfigurationProperty("name", IsRequired = false)]
		public string Name
		{
			get
			{
				return (string)this["name"];
			}
		}

		[ConfigurationProperty("connectionString", IsRequired = false)]
		public string ConnectionString
		{
			get
			{
				return (string)this["connectionString"];
			}
		}

		[ConfigurationProperty("port", IsRequired = false)]
		public int Port
		{
			get
			{
				return (int)this["port"];
			}
		}

		[ConfigurationProperty("passWord", IsRequired = false)]
		public string PassWord
		{
			get
			{
				return (string)this["passWord"];
			}
		}

		[ConfigurationProperty("db", IsRequired = false)]
		public int DB
		{
			get
			{
				return (int)this["db"];
			}
		}
	}
}
