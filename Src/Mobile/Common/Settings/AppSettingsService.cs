using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using MK.Common.Configuration;
using ServiceStack.Text;
using TinyIoC;
using System.Globalization;
using apcurium.MK.Common.Configuration.Helpers;
using apcurium.MK.Common.Extensions;
using Cirrious.CrossCore;

namespace apcurium.MK.Booking.Mobile.Settings
{
    public class AppSettingsService : IAppSettings
    {
		public TaxiHailSetting Data { get; private set; }

        readonly ICacheService _cacheService;
		readonly ILogger _logger;
		const string SettingsCacheKey = "TaxiHailSetting";

		public AppSettingsService (ICacheService cacheService, ILogger logger)
        {
			_logger = logger;
			_cacheService = cacheService;

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
				await RefreshSettingsFromServer();
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
						Dictionary<string,string> values = JsonObject.Parse(serializedData);
						SettingsLoader.InitializeDataObjects (Data, values, _logger);
					}
				}
			}
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
						Dictionary<string,string> values = JsonObject.Parse(serializedData);

						string settingValue = null;
						values.TryGetValue(settingName, out settingValue);

						return settingValue;
					}
				}
				return null;
			}
		}

		private async Task RefreshSettingsFromServer()
		{
			_logger.LogMessage("load settings from server");

			var settingsFromServer = await TinyIoCContainer.Current.Resolve<ConfigurationClientService>().GetSettings();
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
