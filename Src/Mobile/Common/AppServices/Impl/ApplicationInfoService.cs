using System;
using System.Linq;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Threading.Tasks;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class ApplicationInfoService : BaseService, IApplicationInfoService
    {
        private const string AppInfoCacheKey = "ApplicationInfo";

		readonly ILocalization _localize;
		readonly IMessageService _messageService;
		readonly IPackageInfo _packageInfo;

		public ApplicationInfoService(ILocalization localize, IMessageService messageService, IPackageInfo packageInfo )
		{
			_packageInfo = packageInfo;
			_messageService = messageService;
			_localize = localize;
        	
		}

        public async Task<ApplicationInfo> GetAppInfoAsync( )
        {
            var cached = Cache.Get<ApplicationInfo>(AppInfoCacheKey);
            if (cached == null)
            {
                var appInfo = UseServiceClientAsync<ApplicationInfoServiceClient, ApplicationInfo>(service => service.GetAppInfoAsync());
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
			bool isUpToDate = true;
            try
            {
                var app = await GetAppInfoAsync();
				 
				if ( _packageInfo.Version.Count( c=>  c == '.' ) == 2 )
				{
					var v = _packageInfo.Version.Split( '.' ).Take(2).JoinBy(".")+".";
					isUpToDate = app.Version.StartsWith(v);
				}

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

