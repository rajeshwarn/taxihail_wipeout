using System;
using System.Collections.Generic;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;

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

            SetSettings(dictionary);
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public string GetSetting(string key)
        {
            return AppSettings[key];
        }

        public IDictionary<string, string> GetSettings()
        {
            return AppSettings;
        }

        public void SetSettings(IDictionary<string, string> appSettings)
        {
            AppSettings = appSettings;
        }

        public ClientPaymentSetting GetPaymentSettings()
        {
           return new PaymentSetting(Guid.NewGuid());
        }

        private IDictionary<string, string> AppSettings { get; set; }

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
