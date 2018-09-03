using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceTest
{
	class Program
	{
		static void Main(string[] args)
		{
			OfficeService.Main.Instance.OperationRedis();
		}
	}
}
