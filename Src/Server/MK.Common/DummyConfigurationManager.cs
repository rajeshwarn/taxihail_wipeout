#region

using System;
using System.Collections.Generic;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;

#endregion

namespace apcurium.MK.Common
{
    public class DummyConfigManager : IConfigurationManager
    {
        public DummyConfigManager(Dictionary<string, string> dictionary = null)
        {
            if (dictionary == null)
            {
                dictionary = new Dictionary<string, string>();
            }

            AppSettings = dictionary;
        }

        private IDictionary<string, string> AppSettings { get; set; }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public string GetSetting(string key)
        {
            return AppSettings[key];
        }

        public T GetSetting<T>(string key, T defaultValue) where T : struct
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, string> GetSettings()
        {
            return AppSettings;
        }

        public ClientPaymentSettings GetPaymentSettings(bool force = true)
        {
            throw new NotImplementedException();
        }

        public void AddOrSet(string key, string value)
        {
            if (AppSettings.ContainsKey(key))
            {
                AppSettings[key] = value;
            }
            else
            {
                AppSettings.Add(key, value);
            }
        }
    }
}