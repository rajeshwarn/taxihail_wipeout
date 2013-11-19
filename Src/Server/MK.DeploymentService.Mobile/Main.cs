using System;
using DeploymentServiceTools;
using Cupertino;

namespace MK.DeploymentService.Mobile
{
	class MainClass
	{
		public static void Main (string[] args)
		{
//			var test = new Agent ();
//			test.Login ("https://daw.apple.com/cgi-bin/WebObjects/DSAuthWeb.woa/wa/login?&appIdKey=891bd3417a7776362562d2197f89480a8547b108fd934911bcbea0110d07f757&path=%2F%2Fdevcenter%2Fios%2Findex.action");
//			test.Get ("https://developer.apple.com/account/ios/profile/profileList.action?type=limited");

            var service = new DeploymentJobService();
			service.Start();
            while (true) { }
		}
	}
}
