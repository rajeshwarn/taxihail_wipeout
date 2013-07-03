using System;
using System.Linq;
using System.Collections.Generic;

namespace apcurium.MK.Common.Configuration.Impl
{
    public class ConfigurationManager : IConfigurationManager
    {
        private readonly Func<ConfigurationDbContext> _contextFactory;

        public ConfigurationManager(Func<ConfigurationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public string GetSetting(string key)
        {
            string value;

            GetSettings().TryGetValue(key, out value);
            
            return value;
        }

        public IDictionary<string, string> GetSettings()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AppSetting>().ToArray().ToDictionary(kv => kv.Key, kv => kv.Value);
            }
        }

        public void SetSettings(IDictionary<string, string> appSettings)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
        

        public ClientPaymentSettings GetPaymentSettings()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<ServerPaymentSettings>().SingleOrDefault();
            }
        }
    }
}