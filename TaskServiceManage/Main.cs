using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskServiceManage
{
	public partial class Main : Form
	{
		public Main()
		{
			InitializeComponent();
		}
		string serviceFilePath = $"{Application.StartupPath}\\OfficeService.exe";
		string serviceName = "OfficeTaskService";


		/// <summary>
		/// 安装服务
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnInstall_Click(object sender, EventArgs e)
		{
			new Thread(new ThreadStart(new Action(() =>
			{
				if (!CheckServerState().IsExite)
				{
					this.InstallService(serviceFilePath);
				}
				MessageBox.Show("服务已安装");
			}))).Start();
		}

		/// <summary>
		/// 启动服务
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnStart_Click(object sender, EventArgs e)
		{
			new Thread(new ThreadStart(new Action(() =>
			{
				var state = CheckServerState();
				if (!state.IsExite)
				{
					MessageBox.Show("服务已卸载");
				}
				if (state.IsExite && !state.IsStart)
				{
					this.ServiceStart(serviceName);
					MessageBox.Show("服务已启动");
				}
			}))).Start();
		}

		/// <summary>
		/// 停止服务
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnEnd_Click(object sender, EventArgs e)
		{
			new Thread(new ThreadStart(new Action(() =>
			{
				var state = CheckServerState();
				if (!state.IsExite)
				{
					MessageBox.Show("服务已卸载");
				}
				if (state.IsExite && state.IsStart)
				{
					this.ServiceStop(serviceName);
					MessageBox.Show("服务已停止");
				}
			}))).Start();
		}

		/// <summary>
		/// 卸载服务
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnUninstall_Click(object sender, EventArgs e)
		{
			new Thread(new ThreadStart(new Action(() =>
			{
				var state = CheckServerState();
				if (state.IsExite)
				{
					this.ServiceStop(serviceName);
					this.UninstallService(serviceFilePath);

				}
				MessageBox.Show("服务已卸载");
			}))).Start();
		}

		//判断服务是否存在
		private bool IsServiceExisted(string serviceName)
		{
			ServiceController[] services = ServiceController.GetServices();
			foreach (ServiceController sc in services)
			{
				if (sc.ServiceName.ToLower() == serviceName.ToLower())
				{
					return true;
				}
			}
			return false;
		}

		//安装服务
		private void InstallService(string serviceFilePath)
		{
			using (AssemblyInstaller installer = new AssemblyInstaller())
			{
				installer.UseNewContext = true;
				installer.Path = serviceFilePath;
				IDictionary savedState = new Hashtable();
				installer.Install(savedState);
				installer.Commit(savedState);
			}
		}

		//卸载服务
		private void UninstallService(string serviceFilePath)
		{
			using (AssemblyInstaller installer = new AssemblyInstaller())
			{
				installer.UseNewContext = true;
				installer.Path = serviceFilePath;
				installer.Uninstall(null);
			}
		}
		//启动服务
		private void ServiceStart(string serviceName)
		{
			using (ServiceController control = new ServiceController(serviceName))
			{
				if (control.Status == ServiceControllerStatus.Stopped)
				{
					control.Start();
				}
			}
		}

		//停止服务
		private void ServiceStop(string serviceName)
		{
			using (ServiceController control = new ServiceController(serviceName))
			{
				if (control.Status == ServiceControllerStatus.Running)
				{
					control.Stop();
				}
			}
		}

		//查询服务是否存在
		public State CheckServerState()
		{
			ServiceController[] service = ServiceController.GetServices();
			State state = new State();
			for (int i = 0; i < service.Length; i++)
			{
				if (service[i].ServiceName.ToUpper().Equals(serviceName.ToUpper()))
				{
					state.IsExite = true;
					if (service[i].Status == ServiceControllerStatus.Running)
					{
						state.IsStart = true;
						break;
					}
				}
			}
			return state;
		}

		/// <summary>
		/// 服务状态
		/// </summary>
		public class State
		{
			/// <summary>
			/// 是否安装
			/// </summary>
			public bool IsExite { get; set; }

			/// <summary>
			/// 是否关闭
			/// </summary>
			public bool IsStart { get; set; }
		}
	}
}
