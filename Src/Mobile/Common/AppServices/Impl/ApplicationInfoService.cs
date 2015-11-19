using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common;
using apcurium.MK.Common.Diagnostic;
using MK.Common.Android.Extensions;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class ApplicationInfoService : BaseService, IApplicationInfoService
    {
        private const string AppInfoCacheKey = "ApplicationInfo";

		private readonly ILocalization _localize;
		private readonly IMessageService _messageService;
		private readonly IPackageInfo _packageInfo;
		private readonly ICacheService _cacheService;
        private readonly ILogger _logger;

		private DateTime _minimalVersionChecked;
		private const int CheckMinimumSupportedVersionWhenIntervalExpired = 6; // hours

		public ApplicationInfoService(ILocalization localize,
            IMessageService messageService,
            IPackageInfo packageInfo,
            ICacheService cacheService,
            ILogger logger)
		{
			_packageInfo = packageInfo;
			_messageService = messageService;
			_localize = localize;
			_cacheService = cacheService;
		    _logger = logger;
		}

        public async Task<ApplicationInfo> GetAppInfoAsync()
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


        public async Task CheckVersionAsync()
        {
			if ((DateTime.Now - _minimalVersionChecked).TotalHours >= CheckMinimumSupportedVersionWhenIntervalExpired)
			{
				_minimalVersionChecked = DateTime.Now;
			}
			else
			{
				return;
			}

			var isSupported = true;
            
			try
            {
                var appInfo = await GetAppInfoAsync();

				var mobileVersion = new ApplicationVersion(_packageInfo.Version);
                var minimumRequiredVersion = new ApplicationVersion(appInfo.MinimumRequiredAppVersion);

				if (mobileVersion < minimumRequiredVersion)
				{
					isSupported = false;
				}
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithCaller(ex);
            }

            if (!isSupported)
            {
                // App is not supported anymore (also means that an update is available so don't display the other pop-up)
                await _messageService.ShowMessage(_localize["UpdateNoticeTitle"], _localize["UpdateNoticeText"]);
            }
		}
    }
}