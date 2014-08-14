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

namespace apcurium.MK.Booking.Mobile.Settings
{
    public class AppSettingsService : IAppSettings
    {
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

		public void Load()
		{
			//check if the cache has already something
			var data = _cacheService.Get<TaxiHailSetting>(SettingsCacheKey);
			if (data != null)
			{
				Data = data;
			}
			//launch async refresh from the server
			Task.Factory.StartNew(() => RefreshSettingsFromServer());
		}

		public void ChangeServerUrl(string serverUrl)
		{
			Data = new TaxiHailSetting();
			LoadSettingsFromFile();
			Data.ServiceUrl = serverUrl;

			_cacheService.Clear(SettingsCacheKey);
			Task.Factory.StartNew(() => RefreshSettingsFromServer());
		}

		void LoadSettingsFromFile()
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
						string serializedData = reader.ReadToEnd();
						Dictionary<string,string> values = JsonObject.Parse(serializedData);
						SetSettingsValue(values);
					}
				}
			}
		}

		void RefreshSettingsFromServer()
		{
			_logger.LogMessage("load settings from server");
			var service = TinyIoCContainer.Current.Resolve<ConfigurationClientService>();
			IDictionary<string,string> settingsFromServer = service.GetSettings();
			SetSettingsValue(settingsFromServer, "ServiceUrl", "CanChangeServiceUrl");
			SaveSettings();			
		}

		void SaveSettings()
		{
			_cacheService.Set(SettingsCacheKey, Data);
		}

		void SetSettingsValue(IDictionary<string,string> values, params string [] excludedKeys)
		{
			var typeOfSettings = typeof(TaxiHailSetting); 
			foreach (KeyValuePair<string,string> item in values)
			{
				if ((excludedKeys == null ) || (!excludedKeys.Any (key => item.Key.Contains (key)))) {
					try {
#if DEBUG
						_logger.LogMessage ("setting {0} - value {1} ", item.Key, item.Value);
#endif
						var propertyName = item.Key.Contains (".") ? 
					                   item.Key.SplitOnLast ('.') [1]
						               : item.Key;

						var propertyType = typeOfSettings.GetProperty (propertyName);

						if (propertyType == null) {
#if DEBUG
							_logger.LogMessage ("property not found for {0}", item.Key);
#endif
							continue;
						}

						var targetType = IsNullableType (propertyType.PropertyType) ? 
					                 		Nullable.GetUnderlyingType (propertyType.PropertyType) 
					                 		: propertyType.PropertyType;

						object propertyVal = null;
						if (targetType.IsEnum) {
							propertyVal = Enum.Parse (targetType, item.Value);
						} else {
							propertyVal = Convert.ChangeType (item.Value, targetType, CultureInfo.InvariantCulture);	
						}			 
						propertyType.SetValue (Data, propertyVal);
					} catch (Exception e) {
						_logger.LogError (e);
						_logger.LogMessage ("Error can't set value for property {0}, value was {1}", item.Key, item.Value);
					}
				}
			}
		}

		private static bool IsNullableType(Type type)
		{
			return type.IsGenericType 
				&& type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
		}


		public TaxiHailSetting Data { get; private set; }
        
    }
}
