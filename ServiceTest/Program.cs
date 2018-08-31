using OfficeService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceTest
{
	class Program
	{
		static void Main(string[] args)
		{
			var a = ConfigurationManager.GetSection("yuanxinRedisSettings");
			OfficeService.Main.Instance.OperationRedis();
		}
	}
}
