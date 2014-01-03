using System;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class ApplicationInfoService : BaseService, IApplicationInfoService
    {
        private const string AppInfoCacheKey = "ApplicationInfo";

        public async Task<ApplicationInfo> GetAppInfoAsync( )
        {
            var cached = Cache.Get<ApplicationInfo>(AppInfoCacheKey);
            if (cached == null)
            {
                var appInfo = UseServiceClient<ApplicationInfoServiceClient, ApplicationInfo>(service => service.GetAppInfoAsync());
                Cache.Set(AppInfoCacheKey, await appInfo, DateTime.Now.AddHours(1));
                return await appInfo;
            }
            return cached;
        }

        public void ClearAppInfo()
        {
            Cache.Clear (AppInfoCacheKey);
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

            if (!isUpToDate)
            {

                var title = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("AppNeedUpdateTitle");
                var msg = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("AppNeedUpdateMessage");
                var mService = TinyIoCContainer.Current.Resolve<IMessageService>();
#pragma warning disable 4014
                mService.ShowMessage(title, msg);
#pragma warning restore 4014
            }
        }
    }
}

