using apcurium.MK.Common.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Api.Client
{
    public class ConfigurationClientService : BaseServiceClient, IConfigurationManager
    {
        private static IDictionary<string, string> _settings = null;

        public ConfigurationClientService(string url, string sessionId)
            : base(url, sessionId)
        {
        }

        public void Reset()
        {
            _settings = null;
        }

        public string GetSetting(string key)
        {
            if (_settings == null)
            {
                Load();
            }
            return _settings[key];
        }

        private void Load()
        {            
            _settings = Client.Get<IDictionary<string,string>>("/settings");
            _settings.ToString();
        }

        public void SetSetting(string key, string value)
        {            
        }

        public IDictionary<string, string> GetAllSettings()
        {
            if (_settings == null)
            {
                Load();
            }
            return _settings;            
        }
    }
}
