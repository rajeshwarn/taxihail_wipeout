
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Client;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	class ApplicationInfoService : BaseService, IApplicationInfoService
	{
        public ApplicationInfo GetAppInfo()
		{
            ApplicationInfo info = new ApplicationInfo();
			UseServiceClient<ApplicationInfoServiceClient>( service => {
				info = service.GetAppInfo();				
			});
			return info;
		}

		public string GetServerVersion()
		{
			string version = "";
			UseServiceClient<ApplicationInfoServiceClient>( service => {
				ApplicationInfo info = service.GetAppInfo();
				version = info.Version;
			});
			return version;

		}
	}
}

