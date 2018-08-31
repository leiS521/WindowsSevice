using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace OfficeService
{
	static class Program
	{
		/// <summary>
		/// 应用程序的主入口点。
		/// </summary>
		static void Main()
		{
			string assemblyFilePath = Assembly.GetExecutingAssembly().Location;
            string assemblyDirPath = Path.GetDirectoryName(assemblyFilePath);
            string configFilePath = assemblyDirPath + "\\log4net.config";
            XmlConfigurator.ConfigureAndWatch(new FileInfo(configFilePath));

			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[]
			{
				new TaskService()
			};
			ServiceBase.Run(ServicesToRun);
		}
	}
}
