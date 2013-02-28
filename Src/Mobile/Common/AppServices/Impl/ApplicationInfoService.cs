
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Client;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Threading.Tasks;
using System.Threading;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class ApplicationInfoService : BaseService, IApplicationInfoService
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

        public void CheckVersion()
        {

            var t = Task.Factory.StartNew(() =>
            {                
                bool isUpToDate;
                try
                {
                    var app = GetAppInfo();
                    isUpToDate = app.Version.StartsWith("1.4.");
                }
                catch (Exception e)
                {
                    isUpToDate = true;
                }

                if (!isUpToDate)
                {

                    var title = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("AppNeedUpdateTitle");
                    var msg = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("AppNeedUpdateMessage");
                    var mService = TinyIoCContainer.Current.Resolve<IMessageService>();
                    mService.ShowMessage(title, msg);                    
                }
            });



        }


	}
}

