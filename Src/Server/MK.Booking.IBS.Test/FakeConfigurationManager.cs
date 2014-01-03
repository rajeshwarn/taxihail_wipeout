using System;
using System.Collections.Generic;
using System.ComponentModel;
using apcurium.MK.Common.Configuration;

namespace MK.Booking.IBS.Test
{
    public class FakeConfigurationManager: IConfigurationManager
    {
        readonly Dictionary<string, string> _settings = new Dictionary<string, string>()
        {
            {"IBS.WebServicesPassword", "test"},
            {"IBS.WebServicesUrl", "http://apcuriumibs:6928/XDS_IASPI.DLL/soap/"},            
            {"IBS.WebServicesUserName", "taxi"}
        };

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public string GetSetting(string key)
        {
            string val = null;
            if (_settings.TryGetValue(key, out val))
            {
                return val;
            }
            else
            {
                return null;
            }

        }

        public void AddKey(string key, string val)
        {
            _settings.Add(key, val);
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
                
            }
            return defaultValue;
        }

        public IDictionary<string, string> GetSettings()
        {
            return _settings;
        }

        public ClientPaymentSettings GetPaymentSettings(bool cleanCache = false)
        {
            throw new NotImplementedException();
        }
    }
}
