using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using StackExchange.Redis;
using static Seagull2.YuanXin.WebApi.ViewsModel.TaskManage.TaskManageViewModel;
using System.Net;
using System.IO;
using log4net;
using System.Reflection;
using Seagull2.YuanXin.WebApi.Extensions;

namespace OfficeService
{
	/// <summary>
	/// Redis业务类
	/// </summary>
	public class Main
	{
		/// 实例化
		public static readonly Main Instance = new Main();
		public static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		static readonly string url = ConfigurationManager.AppSettings["YuanXinApiUrl"]?.ToString();
		protected const string redisKey = "Seagull2.YuanXin.WebApi.TaskManage";
		public static readonly RedisManager Rm = new RedisManager("ClientRedisHost");

		/// <summary>
		/// 查询Redis数据,对符合条件的数据进行过滤消息提醒
		/// </summary>
		public void OperationRedis()
		{
			try
			{
				var arr = Rm.SetCombine(SetOperation.Union, redisKey, redisKey);//查询redis所有集合
				List<RedisViewModel> list = new List<RedisViewModel>();
				if (arr.Length > 0)
				{
					for (int i = 0; i < arr.Length; i++)
					{
						list.Add(JsonConvert.DeserializeObject<RedisViewModel>(arr[i]));
					}
				}
				if (list.Count > 0)
				{
					list.OrderByDescending(o => o.RemindTime).ToList().ForEach(m =>
					{
						var state = DateCheck(DateTime.Parse(m.RemindTime));
						if (state == EnumRemindTimeStatus.Valid)
						{
							return;
						}
						if (state == EnumRemindTimeStatus.Push)
						{
							//执行推送
							HttpApi("API地址", JsonConvert.SerializeObject(m), "post");
							//从redis删除元素
							log.Info($"开始删除Redis");
							Rm.SetRemove(redisKey, JsonConvert.SerializeObject(m));
							log.Info($"推送成功，推送参数{m.Code}");
							return;
						}
						if (state == EnumRemindTimeStatus.Expire)
						{
							//从redis删除元素
							Rm.SetRemove(redisKey, JsonConvert.SerializeObject(m));
							log.Info($"删除过期redis,数据{m.Code}");
							return;
						}
					});
				}
			}
			catch (Exception ex)
			{
				log.Error($"OfficeService服务异常，" + ex.ToString());
			}
		}

		/// <summary>
		/// 日期校验
		/// </summary>
		/// <param name="remindTime"></param>
		/// <returns></returns>
		private EnumRemindTimeStatus DateCheck(DateTime remindTime)
		{
			if (DateTime.Now > remindTime) return EnumRemindTimeStatus.Expire;
			var nowTime = DateTime.Now;
			TimeSpan ts = remindTime - nowTime;
			return ts.TotalMinutes <= 1 ? EnumRemindTimeStatus.Push : EnumRemindTimeStatus.Valid;
		}

		/// <summary>
		/// 调用api返回json
		/// </summary>
		/// <param name="url">api地址</param>
		/// <param name="jsonstr">接收参数</param>
		/// <param name="type">类型</param>
		/// <returns></returns>
		private string HttpApi(string url, string jsonstr, string type)
		{
			Encoding encoding = Encoding.UTF8;
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);//webrequest请求api地址
			request.Accept = "text/html,application/xhtml+xml,*/*";
			request.ContentType = "application/json";
			request.Method = type.ToUpper().ToString();//get或者post
			byte[] buffer = encoding.GetBytes(jsonstr);
			request.ContentLength = buffer.Length;
			request.GetRequestStream().Write(buffer, 0, buffer.Length);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
			{
				return reader.ReadToEnd();
			}
		}

		/// <summary>
		/// 提醒时间状态枚举
		/// </summary>
		private enum EnumRemindTimeStatus
		{
			/// <summary>
			/// 过期
			/// </summary>
			Expire = 0,

			/// <summary>
			/// 发送
			/// </summary>
			Push = 1,

			/// <summary>
			/// 有效的
			/// </summary>
			Valid = 2
		}

	}
}
