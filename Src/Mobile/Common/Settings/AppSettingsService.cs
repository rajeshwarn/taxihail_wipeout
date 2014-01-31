using System;
using System.Linq;
using apcurium.MK.Booking.Mobile.Infrastructure;
using MK.Common.Configuration;
using ServiceStack.Text;
using System.IO;
using apcurium.MK.Common.Configuration;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common.Diagnostic;
using TinyIoC;
using System.Threading.Tasks;

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
			if (data == null)
			{
				//first run or cache cleared, fetch settings from server
				RefreshSettingsFromServer();
			}
			else
			{
				//already got settings launch async refresh
				Task.Factory.StartNew(() => RefreshSettingsFromServer());
			}
		}

		void LoadSettingsFromFile()
		{
			using (var stream = GetType().Assembly.GetManifestResourceStream(GetType ().Assembly.GetManifestResourceNames().FirstOrDefault(x => x.Contains("Settings.json")))) 
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
			var service = TinyIoCContainer.Current.Resolve<ConfigurationClientService>();
			IDictionary<string,string> settingsFromServer = service.GetSettings();
			SetSettingsValue(settingsFromServer);
			SaveSettings();			
		}

		void SaveSettings()
		{
			_cacheService.Set(SettingsCacheKey, Data);
		}

		void SetSettingsValue(IDictionary<string,string> values)
		{
			var typeOfSettings = typeof(TaxiHailSetting); 
			foreach (KeyValuePair<string,string> item in values)
			{
				try
				{
					_logger.LogMessage("setting {0} - value {1}", item.Key, item.Value);
					var propertyName = item.Key.Contains(".") ? 
					                   item.Key.SplitOnLast('.')[1]
						               : item.Key;

					var propertyType = typeOfSettings.GetProperty(propertyName);
					var targetType = IsNullableType(propertyType.PropertyType) ? 
					                 		Nullable.GetUnderlyingType(propertyType.PropertyType) 
					                 		: propertyType.PropertyType;

					var propertyVal = Convert.ChangeType(item.Value, targetType);					 
					propertyType.SetValue(Data, propertyVal);
				}
				catch(Exception e)
				{
					_logger.LogError(e);
					_logger.LogMessage("Error can't set value for property {0}, value was {1}", item.Key, item.Value);
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
