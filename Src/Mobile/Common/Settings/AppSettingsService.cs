using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using TinyIoC;
using apcurium.MK.Common.Configuration.Helpers;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Services;
using Cirrious.CrossCore;

namespace apcurium.MK.Booking.Mobile.Settings
{
    public class AppSettingsService : IAppSettings
    {
		public TaxiHailSetting Data { get; private set; }

		private bool _settingsFileLoaded;

        private readonly ICacheService _cacheService;
		private readonly ILogger _logger;
        private readonly ICryptographyService _cryptographyService;

		const string SettingsCacheKey = "TaxiHailSetting";

		public AppSettingsService (ICacheService cacheService, ILogger logger, ICryptographyService cryptographyService)
        {
			_logger = logger;
			_cacheService = cacheService;
		    _cryptographyService = cryptographyService;

            Data = new TaxiHailSetting();

			//bare minimum for the app to work (server url etc.)
			LoadSettingsFromFile();
		}

        public async Task Load()
		{
			//check if the cache has already something
            // We also verify if the application name is present to make sure the cached settings are in a valid state.
			var data = _cacheService.Get<TaxiHailSetting>(SettingsCacheKey);
			if (data != null && data.TaxiHail.ApplicationName.HasValue())
			{
				// Use cached settings until settings are done loading
				if (!data.CanChangeServiceUrl)
				{
					// Always use service URL from file, not from cache in case it changes
					var bundledServiceUrl = GetSettingFromFile("ServiceUrl");

					data.ServiceUrl = bundledServiceUrl;
				}

				Data = data;

                // Update settings asynchronously. NB: ServiceUrl is never returned from the server settings
				Task.Run(() => RefreshSettingsFromServer());
			    
			}
            else
            {
                // No settings in cache, need to load them synchronously 
                await RefreshSettingsFromServer();
            }
		}

		public async Task ChangeServerUrl(string serverUrl)
		{
			// Reset cache
			Data = new TaxiHailSetting();
			LoadSettingsFromFile();
			Data.ServiceUrl = serverUrl;

			_cacheService.Clear(SettingsCacheKey);

			try
			{
				await RefreshSettingsFromServer(true);

				await Mvx.Resolve<IMessageService>().ShowMessage("Apps needs to restart", "Server url succesfully updated, press to quit the application");
				Mvx.Resolve<IQuitApplicationService>().Quit ();
			}
			catch
			{
				// server url probably not good, revert
				Mvx.Resolve<IMessageService>().ShowMessage("Error", "Server not found, reverting server url");

				Data = new TaxiHailSetting();
				LoadSettingsFromFile();

				_cacheService.Clear(SettingsCacheKey);

				RefreshSettingsFromServer();
			}
		}

		public string GetServiceUrl()
		{
			if (_settingsFileLoaded)
			{
				// settings file is loaded, we can rely on CanChangeServiceUrl value
				if (Data.CanChangeServiceUrl)
				{
					// we have loaded the settings file but we allow to change the service url
					// check cache otherwise return the service url in data
					var cachedData = _cacheService.Get<TaxiHailSetting>(SettingsCacheKey);
					if (cachedData != null && cachedData.TaxiHail.ApplicationName.HasValue())
					{
						return cachedData.ServiceUrl;
					}
				}

				// we have loaded the settings file and we cannot change the service url
				// or we don't have anything in cache therefore the service url in data is the good one
				return Data.ServiceUrl;
			}
			else
			{
				// settings file is not yet loaded, check the cache for CanChangeServiceUrl value
				var cachedData = _cacheService.Get<TaxiHailSetting>(SettingsCacheKey);
				if (cachedData != null && cachedData.TaxiHail.ApplicationName.HasValue() && cachedData.CanChangeServiceUrl)
				{
					// we allow to change the service url, assume that user changed it and he wants the latest one he typed
					return cachedData.ServiceUrl;
				}

				return GetSettingFromFile("ServiceUrl");
			}
		}

		private void LoadSettingsFromFile()
		{
			_logger.LogMessage("load settings from file");
			using (var stream = GetType().Assembly.GetManifestResourceStream(GetType ().Assembly
						.GetManifestResourceNames()
						.FirstOrDefault(x => x.Contains("Settings.json")))) 
			{
				if (stream != null)
				{
					using (var reader = new StreamReader(stream))
					{
						var serializedData = reader.ReadToEnd();
					    
						var values = serializedData.FromJson<Dictionary<string, string>>();
                        SettingsLoader.InitializeDataObjects (Data, values, _logger);
					}
				}
			}

			if (Data.CanChangeServiceUrl)
			{
				var data = _cacheService.Get<TaxiHailSetting>(SettingsCacheKey);
				if (data != null && data.TaxiHail.ApplicationName.HasValue())
				{
					if (Data.ServiceUrl != data.ServiceUrl)
					{
						_logger.LogMessage("CanChangeServiceUrl: url different in cache, using cached value {0}", data.ServiceUrl);
						Data.ServiceUrl = data.ServiceUrl;
					}
				}
			}

			_settingsFileLoaded = true;
		}

		private string GetSettingFromFile(string settingName)
		{
			_logger.LogMessage("loading setting {0} from file", settingName);

			using (var stream = GetType().Assembly.GetManifestResourceStream(GetType ().Assembly
						.GetManifestResourceNames()
						.FirstOrDefault(x => x.Contains("Settings.json")))) 
			{
				if (stream != null)
				{
					using (var reader = new StreamReader(stream))
					{
						var serializedData = reader.ReadToEnd();
						var values = serializedData.FromJson<Dictionary<string, string>>();

						string settingValue = null;
						values.TryGetValue(settingName, out settingValue);

						return settingValue;
					}
				}
				return null;
			}
		}

		private async Task RefreshSettingsFromServer(bool getSettingsShouldThrowExceptionIfError = false)
		{
			_logger.LogMessage("load settings from server");

			var settingsFromServer = await TinyIoCContainer.Current.Resolve<ConfigurationClientService>().GetSettings(getSettingsShouldThrowExceptionIfError);
            _cryptographyService.SwitchEncryptionStringsDictionary(Data.GetType(), null, settingsFromServer, false);

            SettingsLoader.InitializeDataObjects(Data, settingsFromServer, _logger, new[] { "ServiceUrl", "CanChangeServiceUrl" });

			SaveSettings();			
		}

		private void SaveSettings()
		{
			_cacheService.Set(SettingsCacheKey, Data);
		}

		private static bool IsNullableType(Type type)
		{
			return type.IsGenericType 
				&& type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

	    public void SetAppleTestAccountMode(bool isAppleTestAccountUsed)
	    {
		    Data.AppleTestAccountUsed = isAppleTestAccountUsed;

			SaveSettings();
	    }
    }
}
