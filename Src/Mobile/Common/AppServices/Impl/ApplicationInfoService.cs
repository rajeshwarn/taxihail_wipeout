using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common;
using apcurium.MK.Common.Diagnostic;

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

        private bool _didCheckForUpdates;
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


        public async Task CheckVersionAsync(VersionCheckTypes versionCheckType)
        {
            if (versionCheckType == VersionCheckTypes.CheckForUpdates)
			{
                if (!_didCheckForUpdates)
				{
                    _didCheckForUpdates = true;
				}
				else
				{
					return;
				}
			}

            if (versionCheckType == VersionCheckTypes.CheckForMinimumSupportedVersion)
			{
				if ((DateTime.Now - _minimalVersionChecked).TotalHours >= CheckMinimumSupportedVersionWhenIntervalExpired)
				{
					_minimalVersionChecked = DateTime.Now;
				}
				else
				{
					return;
				}
			}

			var isUpToDate = true;
			var isSupported = true;
            
			try
            {
                var app = await GetAppInfoAsync();

				var mobileVersion = new ApplicationVersion(_packageInfo.Version);
				var serverVersion = new ApplicationVersion(app.Version);
				var minimumRequiredVersion = new ApplicationVersion(app.MinimumRequiredAppVersion);

				if (mobileVersion < serverVersion)
				{
					isUpToDate = false;
				}

				if (mobileVersion < minimumRequiredVersion)
				{
					isSupported = false;
				}
            }
            catch (Exception ex)
            {
                _logger.LogMessage("An error occured when trying to check the minimum app version.");
                _logger.LogError(ex);
            }

            if (versionCheckType == VersionCheckTypes.CheckForUpdates && !isUpToDate)
            {
				await _messageService.ShowMessage(_localize["AppNeedUpdateTitle"], _localize["AppNeedUpdateMessage"]);
            }

            if (versionCheckType == VersionCheckTypes.CheckForMinimumSupportedVersion && !isSupported)
			{
				await _messageService.ShowMessage(_localize["UpdateNoticeTitle"], _localize["UpdateNoticeText"]);
			}
		}
    }
}