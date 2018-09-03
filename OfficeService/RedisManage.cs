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
		#region
		/// init
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

		#region
		///已过期的方法
		/// <summary>
		/// 保存到redis
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">key</param>
		/// <param name="t">对象</param>
		/// <returns></returns>
		[Obsolete("StringSetAsync")]
		public bool Save<T>(string key, T t)
		{
			return this.Save<T>(key, t, null);
		}
		/// <summary>
		/// 保存到redis
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">key</param>
		/// <param name="t">对象</param>
		/// <param name="expiredTime">过期时间</param>
		/// <returns></returns>
		[Obsolete("StringSetAsync")]
		public bool Save<T>(string key, T t, Nullable<TimeSpan> expiredTime)
		{
			RedisValue rValue = this.Serialize(t);
			if (!rValue.IsNullOrEmpty)
			{
				return this.db.StringSet(key, rValue, expiredTime);
			}
			return false;
		}

		[Obsolete("StringGetAsync")]
		public T Get<T>(string key)
		{
			if (!string.IsNullOrEmpty(key))
			{
				RedisValue reValue = this.db.StringGet(key); ;
				if (!reValue.IsNullOrEmpty)
				{
					return this.DeSerialize<T>(reValue);
				}
			}
			return default(T);
		}
		[Obsolete("")]

		public byte[] Get(string key)
		{
			if (!string.IsNullOrEmpty(key))
			{
				RedisValue reValue = this.db.StringGet(key); ;
				if (!reValue.IsNullOrEmpty)
				{
					return reValue;
				}
			}
			return null;
		}
		/// <summary>
		/// 重命名
		/// </summary>
		/// <param name="oldKey"></param>
		/// <param name="newKey"></param>
		/// <returns></returns>
		public bool ReNameNx(string oldKey, string newKey)
		{
			return db.KeyRename(oldKey, newKey);
		}

		/// <summary>
		/// 检测key是否存在
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		[Obsolete("KeyExistsAsync")]
		public bool HasKey(string key)
		{
			return db.KeyExists(key);
		}
		/// <summary>
		/// 检测是否过期
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		[Obsolete("KeyTimeToLiveAsync")]
		public bool IsTtl(string key)
		{
			TimeSpan? keyTime = this.db.KeyTimeToLive(key);
			if (keyTime.HasValue)
			{
				return keyTime.Value.Seconds > 0;
			}

			return false;
		}
		/// <summary>
		/// 删除指定管理
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		[Obsolete("DeleteKeyAsync")]
		public bool Delete(string key)
		{
			return this.db.KeyDelete(key);
		}
		/// <summary>
		/// 添加一个项到内部的List
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		[Obsolete("AddItemToListAsync")]
		public void AddItemToList<T>(string key, T value)
		{
			RedisValue rdValue = this.Serialize(value);
			this.db.ListRightPush(key, rdValue);
			//todo:ydz
			// _redisClient.AddItemToList(key, value);
		}

		[Obsolete("KeyExpireAsync")]
		public bool SetExpireByKey(string key, int seconds)
		{
			return this.db.KeyExpire(key, TimeSpan.FromSeconds(seconds));
		}
		[Obsolete("DeleteKeyAsync")]
		public Task<bool> DeleteAll(List<string> keys)
		{
			var tran = db.CreateTransaction();
			foreach (var key in keys)
			{
				tran.KeyDeleteAsync(key);
			}
			return tran.ExecuteAsync();
		}
		[Obsolete("GetAllItemsFromListAsync")]
		public List<T> GetAllItemsFromList<T>(string key)
		{
			List<T> items = new List<T>();
			RedisValue[] values = this.db.ListRangeAsync(key).Result;
			if (values.Length > 0)
			{
				foreach (RedisValue item in values)
				{
					items.Add(this.DeSerialize<T>(item));
				}
			}
			return items;
		}
		/// <summary>
		/// Removes and returns the first element of the list stored at key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		[Obsolete("ListLeftPopAsync")]
		public string ListLeftPop(string key)
		{
			return db.ListLeftPopAsync(key).Result;
		}
		[Obsolete("ListLengthAsync")]
		public int ListLength(string key)
		{
			return Convert.ToInt32(db.ListLength(key));
		}
		[Obsolete("HashSetAsync(RedisKey key, RedisValue[] fields, RedisValue[] values)")]
		public Task<bool> HashSetAsync(string key, string[] fields, int[] values)
		{
			var tran = db.CreateTransaction(true);
			for (var i = 0; i < fields.Length; i++)
			{
				tran.HashSetAsync(key, fields[i], values[i]);
			}
			return tran.ExecuteAsync();
		}
		[Obsolete("PublishAsync")]
		public Task<bool> PushMessageAsync(string channel, string[] messgae)
		{
			var tran = db.CreateTransaction(true);
			foreach (var item in messgae)
			{
				log("PushMessageAsync:" + item);
				tran.PublishAsync(channel, item);
			}
			return tran.ExecuteAsync();
		}
		private void log(string msg)
		{
			StreamWriter sw1 = File.AppendText(@"D:\WebApp\YuanXinIM\redis.txt");
			string w1 = msg + "\r\n";
			sw1.Write(w1);
			sw1.Close();
		}
		public Task<bool> PushMessageAsync(string channel, string messgae)
		{
			var tran = db.CreateTransaction(true);

			tran.PublishAsync(channel, messgae);

			return tran.ExecuteAsync();
		}
		//todo：此处应该将序列化抽到外部处理
		public RedisValue Serialize<T>(T data)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Serializer.Serialize(ms, data);
				return ms.ToArray();
			}
		}
		public T DeSerialize<T>(RedisValue value)
		{
			using (MemoryStream ms = new MemoryStream(value))
			{
				return Serializer.Deserialize<T>(ms);
			}
		}
		#endregion

		/// <summary>
		/// Save数据
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
		/// 获取数据
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
		/// Save数据
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
		/// 取数据
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
		/// hashset
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
		/// hashset
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
		public Task<RedisValue[]> HashGetAsync(RedisKey key, RedisValue[] fields)
		{
			return db.HashGetAsync(key, fields);
		}

		public Task<HashEntry[]> HashGetAllAsync(RedisKey key)
		{
			return db.HashGetAllAsync(key);
		}

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
