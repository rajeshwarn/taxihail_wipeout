using System;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Client.Cmt
{
	public class CmtApplicationInfoServiceClient : IApplicationInfoServiceClient
	{

		public ApplicationInfo GetAppInfo()
		{
			var result = new ApplicationInfo{
				SiteName = "CMT",
				Version = "1.0.0"
			};
			return result;
		}
		
	}
}

