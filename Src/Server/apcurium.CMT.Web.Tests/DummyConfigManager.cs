using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Configuration;

namespace apcurium.CMT.Web.Tests
{
    public class DummyConfigManager : IConfigurationManager
    {
        public DummyConfigManager(Dictionary<string, string> dictionary)
        {
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

        private IDictionary<string, string> AppSettings { get; set; }

        public void AddOrSet(string key, string value)
        {
            if (AppSettings.ContainsKey(key))
            {
                AppSettings[key] = value;
            }
            else
            {
                AppSettings.Add(key,value);
            }
        }
    }
}
