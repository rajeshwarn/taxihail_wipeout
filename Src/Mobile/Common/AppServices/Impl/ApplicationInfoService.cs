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

		readonly ILocalization _localize;
		readonly IMessageService _messageService;

		public ApplicationInfoService(ILocalization localize, IMessageService messageService)
		{
			_messageService = messageService;
			_localize = localize;
        	
		}

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

				var title = _localize["AppNeedUpdateTitle"];
				var msg = _localize["AppNeedUpdateMessage"];
				await _messageService.ShowMessage(title, msg);
            }
        }
    }
}

