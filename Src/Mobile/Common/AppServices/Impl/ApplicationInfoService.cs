using System;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class ApplicationInfoService : BaseService, IApplicationInfoService
    {
        private const string AppInfoCacheKey = "ApplicationInfo";

		readonly ILocalization _localize;
		readonly IMessageService _messageService;
		readonly IPackageInfo _packageInfo;
		readonly ICacheService _cacheService;

		public ApplicationInfoService(ILocalization localize, 
									IMessageService messageService, 
									IPackageInfo packageInfo,
									ICacheService cacheService)
		{
			_packageInfo = packageInfo;
			_messageService = messageService;
			_localize = localize;
			_cacheService = cacheService;
		}

        public async Task<ApplicationInfo> GetAppInfoAsync( )
        {
			var cached = _cacheService.Get<ApplicationInfo>(AppInfoCacheKey);
            if (cached == null)
            {
                var appInfo = UseServiceClientAsync<ApplicationInfoServiceClient, ApplicationInfo>(service => service.GetAppInfoAsync());
				_cacheService.Set(AppInfoCacheKey, await appInfo, DateTime.Now.AddHours(1));
                return await appInfo;
            }
            return cached;
        }

        public void ClearAppInfo()
        {
			_cacheService.Clear (AppInfoCacheKey);
        }
        
        
        public async void CheckVersionAsync()
        {
			var isUpToDate = true;
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

