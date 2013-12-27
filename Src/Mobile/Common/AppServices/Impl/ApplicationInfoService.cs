
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
        private const string _appInfoCacheKey = "ApplicationInfo";

        public async Task<ApplicationInfo> GetAppInfoAsync( )
        {
            var cached = Cache.Get<ApplicationInfo>(_appInfoCacheKey);
            if (cached == null)
            {
                var appInfo = UseServiceClient<ApplicationInfoServiceClient, ApplicationInfo>(service => service.GetAppInfoAsync());
                Cache.Set<ApplicationInfo>(_appInfoCacheKey, await appInfo, DateTime.Now.AddHours(1));
                return await appInfo;
            }
            return cached;
        }

        public void ClearAppInfo()
        {
            Cache.Clear (_appInfoCacheKey);
        }
        
        
        public async void CheckVersionAsync()
        {

            bool isUpToDate;
            try
            {
                var app = await GetAppInfoAsync();
                isUpToDate = app.Version.StartsWith("1.4.");
            }
            catch
            {
                isUpToDate = true;
            }

#if DEBUG
            isUpToDate = true;
#endif

            if (!isUpToDate)
            {

                var title = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("AppNeedUpdateTitle");
                var msg = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("AppNeedUpdateMessage");
                var mService = TinyIoCContainer.Current.Resolve<IMessageService>();
                mService.ShowMessage(title, msg);
            }
        }
    }
}

