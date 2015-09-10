using System;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class ApplicationInfoService : BaseService, IApplicationInfoService
    {
        private const string AppInfoCacheKey = "ApplicationInfo";

		readonly ILocalization _localize;
		readonly IMessageService _messageService;
		readonly IPackageInfo _packageInfo;
		readonly ICacheService _cacheService;

		bool updatesChecked = false;
		DateTime minimalVersionChecked;
		const int CheckMinimumSupportedVersionWhenIntervalExpired = 6; // hours

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


		public async Task CheckVersionAsync(VersionCheck versionCheck)
        {
			apcurium.MK.Common.Diagnostic.ILogger log = TinyIoC.TinyIoCContainer.Current.Resolve<apcurium.MK.Common.Diagnostic.ILogger>();

			if (versionCheck == VersionCheck.CheckUpdates)
			{
				if (!updatesChecked)
				{
					updatesChecked = true;
				}
				else
				{
					return;
				}
			}

			if (versionCheck == VersionCheck.CheckMinimumSupportedVersion)
			{
				if ((DateTime.Now - minimalVersionChecked).TotalHours >= CheckMinimumSupportedVersionWhenIntervalExpired)
				{
					minimalVersionChecked = DateTime.Now;
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

				ApplicationVersion mobileVersion = new ApplicationVersion(_packageInfo.Version);
				ApplicationVersion serverVersion = new ApplicationVersion(app.Version);
				ApplicationVersion minimumRequiredVersion = new ApplicationVersion(app.MinimumRequiredAppVersion);

				if (mobileVersion < serverVersion)
				{
					isUpToDate = false;
				}

				if (mobileVersion < minimumRequiredVersion)
				{
					isSupported = false;
				}
            }
            catch
            {
            }

            if (versionCheck == VersionCheck.CheckUpdates && !isUpToDate)
            {
				var title = _localize["AppNeedUpdateTitle"];
				var msg = _localize["AppNeedUpdateMessage"];
				await _messageService.ShowMessage(title, msg);
            }
			
			if (versionCheck == VersionCheck.CheckMinimumSupportedVersion && !isSupported)
			{
				var title = _localize["UpdateNoticeTitle"];
				var msg = _localize["UpdateNoticeText"];
				await _messageService.ShowMessage(title, msg);
			}
		}
    }
}