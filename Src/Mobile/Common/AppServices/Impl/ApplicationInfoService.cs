
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
		public string GetServerName()
		{
			string serverName = "";
			UseServiceClient<ApplicationInfoServiceClient>( service => {
				ApplicationInfo info = service.GetAppInfo();
				serverName = info.SiteName;
			});
			return serverName;
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

