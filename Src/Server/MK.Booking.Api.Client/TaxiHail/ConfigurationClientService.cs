#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using Direction = apcurium.MK.Common.Entity.DirectionSetting;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class ConfigurationClientService : BaseServiceClient, IConfigurationManager
    {
        private static Dictionary<string, string> _settings;
        private static readonly object LockObject = new object();
        private static ClientPaymentSettings _cachedSettings;
        private readonly ILogger _logger;

        public ConfigurationClientService(string url, string sessionId, ILogger logger, string userAgent)
            : base(url, sessionId, userAgent)
        {
            _logger = logger;
        }

        public void Reset()
        {
            _settings = null;
        }

        public string GetSetting(string key)
        {
            if ((_settings == null) || (!_settings.ContainsKey(key)))
            {
                Load();
            }

            if (_settings != null && !_settings.ContainsKey(key))
            {
                Console.WriteLine("Missing Key!!!Missing Key!!!Missing Key!!!Missing Key!!!Missing Key!!!Missing Key!!!");
                Console.WriteLine("!!!" + key + "!!!" + key + "!!!" + key + "!!!" + key + "!!!" + key + "!!!" + key +
                                  "!!!" + key + "!!!" + key);
                return null;
            }

            return _settings !=null ? _settings[key] : null;
        }

        public T GetSetting<T>(string key, T defaultValue) where T : struct
        {
            var value = GetSetting(key);
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;

            var converter = GetConverter<T>();

            try
            {
                var convertFromInvariantString = converter.ConvertFromInvariantString(value);
                if (convertFromInvariantString != null)
                    return (T) convertFromInvariantString;
            }
            catch
            {
                _logger.LogMessage("Could not convert setting " + key + " to " + typeof (T).Name);
            }
            return defaultValue;
        }

        public IDictionary<string, string> GetSettings()
        {
            if (_settings == null)
            {
                Load();
            }
            return _settings;
        }


        public ClientPaymentSettings GetPaymentSettings(bool cleanCache = false)
        {
            if (_cachedSettings == null || cleanCache)
            {
                _cachedSettings = Client.Get(new PaymentSettingsRequest()).ClientPaymentSettings;
            }
            return _cachedSettings;
        }

        private TypeConverter GetConverter<T>()
        {
            //TypeDescriptor.GetConverter(defaultValue); doesn't work on the mobile device because the constructor is removed 
            //The actual type is not referenced so the linker removes it 

            var t = typeof (T);
            if (t == typeof (bool))
            {
                return new BooleanConverter();
            }
            if (t == typeof (double))
            {
                return new DoubleConverter();
            }
            if (t.BaseType != null)
            {
                if (t.BaseType == typeof (Enum) && t == typeof (Direction.TarifMode))
                {
                    return new EnumConverter(typeof (Direction.TarifMode));
                }
            }

            _logger.LogMessage("Could not convert setting to " + typeof (T).Name);
            throw new InvalidOperationException("Type " + typeof (T).Name + " has no type converter");
        }

        private void Load()
        {
            lock (LockObject)
            {
                if (_settings == null)
                {
                    _settings = new Dictionary<string, string>();


                    var settings = Client.Get<Dictionary<string, string>>("/settings");
                    settings.ForEach(s => _settings.Add(s.Key, s.Value));
                }
            }
        }
    }
}