using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class ConfigurationClientService : BaseServiceClient, IConfigurationManager
    {
        private static Dictionary<string, string> _settings = null;

        public ConfigurationClientService (string url, string sessionId)
            : base(url, sessionId)
        {
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
                return null;
            }

            return _settings [key];
        }

        public T GetSetting<T>(string key, T defaultValue) where T : struct
        {
            var value = GetSetting(key);
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;
            var converter = TypeDescriptor.GetConverter(defaultValue);
            if (converter == null) throw new InvalidOperationException("Type " + typeof(T).Name + " has no type converter");
            try
            {
                return (T)converter.ConvertFromInvariantString(value);
            }
            catch
            {
                Trace.WriteLine("Could not convert setting " + key + " to " + typeof(T).Name);
            }
            return defaultValue;
        }

        private void Load ()
        {            
            var settings = Client.Get<Dictionary<string,string>> ("/settings");
            var dict = new Dictionary<string, string> ();
            settings.ForEach (s => dict.Add (s.Key, s.Value));
            _settings = dict;
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
