using System;
using System.Collections.Generic;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
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
            if ( (_settings == null) || ( !_settings.ContainsKey( key ) ) )
            {
                Load();
            }
            return _settings[key];
        }

        private void Load()
        {            

            var settings = Client.Get<Dictionary<string,string>>("/settings");
            _settings = new Dictionary<string, string>();
            settings.ForEach( s=> _settings.Add( s.Key, s.Value ));
            _settings.ToString();
        }

        public void SetSetting(string key, string value)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, string> GetSettings()
        {
            if (_settings == null)
            {
                Load();
            }
            return _settings;            
        }

        public void SetSettings(IDictionary<string, string> appSettings)
        {
            throw new NotImplementedException();
        }
    }
}
