using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Common.Diagnostic;



namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class ConfigurationClientService : BaseServiceClient, IConfigurationManager
    {
        private static Dictionary<string, string> _settings = null;
		readonly ILogger _logger;
        private static object lockObject = new object ();

        public ConfigurationClientService (string url, string sessionId, ILogger logger, string userAgent)
            : base(url, sessionId, userAgent)
        {
			this._logger = logger;
        }

        public void Reset ()
        {
            _settings = null;
        }

        public string GetSetting (string key)
        {
            if ((_settings == null) || (!_settings.ContainsKey (key))) {
                Load ();
            }

            if (!_settings.ContainsKey (key)) {
                Console.WriteLine( "Missing Key!!!Missing Key!!!Missing Key!!!Missing Key!!!Missing Key!!!Missing Key!!!" );
                Console.WriteLine( "!!!"+key+"!!!"+key+"!!!"+key+"!!!"+key+"!!!"+key+"!!!"+key+"!!!"+key+"!!!"+key );
                return null;
            }

            return _settings [key];
        }

        private TypeConverter GetConverter<T>()
        {
            //TypeDescriptor.GetConverter(defaultValue); doesn't work on the mobile device because the constructor is removed 
            //The actual type is not referenced so the linker removes it 

            var t = typeof(T);
            if (t.Equals(typeof(bool)))
            {
                return new BooleanConverter();
            }
            else if (t.Equals(typeof(double)))
            {
                return new  DoubleConverter();
            }

            _logger.LogMessage("Could not convert setting to " + typeof(T).Name);
            throw new InvalidOperationException("Type " + typeof(T).Name + " has no type converter");
        }

        public T GetSetting<T>(string key, T defaultValue) where T : struct
        {
            var value = GetSetting(key);
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;

            TypeConverter converter = GetConverter<T>();

            try
            {
                return (T)converter.ConvertFromInvariantString(value);
            }
            catch
            {
                _logger.LogMessage("Could not convert setting " + key + " to " + typeof(T).Name);
            }
            return defaultValue;
        }

        private void Load ()
        {            
            lock (lockObject) {
                if (_settings == null) {
                    _settings = new Dictionary<string, string> ();

                    using (_logger.StartStopwatch("Fetching Settings"))
                    {
                        var settings = Client.Get<Dictionary<string,string>>("/settings");
                        settings.ForEach (s => _settings.Add (s.Key, s.Value));
                    }
                }
            }


        }

        public IDictionary<string, string> GetSettings ()
        {
            if (_settings == null) {
                Load ();
            }
            return _settings;            
        }




		ClientPaymentSettings _cachedSettings = null;
        public ClientPaymentSettings GetPaymentSettings(bool cleanCache = false)
		{
			if(_cachedSettings == null || cleanCache)
			{
				_cachedSettings = Client.Get (new PaymentSettingsRequest ()).ClientPaymentSettings;
			}
			return _cachedSettings;
        }
    }
}
