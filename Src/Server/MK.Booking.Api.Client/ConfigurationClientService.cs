using apcurium.MK.Common.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Client
{
    public class ConfigurationClientService : BaseServiceClient, IConfigurationManager
    {
        private static Dictionary<string, string> _settings = null;

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

            var settings = Client.Get<AppSetting[]>("/settings");
            _settings = new Dictionary<string, string>();
            settings.ForEach( s=> _settings.Add( s.Key, s.Value ));
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
