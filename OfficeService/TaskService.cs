using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace OfficeService
{
	public partial class TaskService : ServiceBase
	{
		public TaskService()
		{
			InitializeComponent();
		}
		System.Timers.Timer timer;  //计时器
		protected override void OnStart(string[] args)
		{
			Main.log.Info("服务启动");
			timer = new System.Timers.Timer();

			timer.Interval = 55 * 1000;  //设置计时器事件间隔执行时间

			timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_RunMan);

			timer.Enabled = true;
			timer.Start();
		}

		protected override void OnStop()
		{
			Main.log.Info("服务停止");
		}

		//每分钟执行一次代码
		public void timer_RunMan(object source, System.Timers.ElapsedEventArgs e)
		{
			try
			{
				System.Timers.Timer tt = (System.Timers.Timer)source;
				tt.Enabled = false;
				Main.Instance.OperationRedis();
				tt.Enabled = true;
			}
			catch (Exception err)
			{
				Main.log.Error(err.ToString());
			}
		}
	}
}
