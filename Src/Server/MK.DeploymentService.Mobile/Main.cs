using System;

namespace MK.DeploymentService.Mobile
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var service = new DeploymentJobService();
			service.Start();
			Console.ReadKey();
		}
	}
}
