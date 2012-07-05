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

        private Dictionary<string, string> _settings = null;

        public string GetSetting(string key)
        {
            if (_settings == null)
            {
                Load();
            }
            return _settings[key];
        }

        public void SetSetting(string key, string value)
        {
            if (_settings == null)
            {
                Load();
            }
            _settings[key] = value;
            Save(key, value);
        }

        private void Save(string key, string value)
        {
            using (var context = _contextFactory.Invoke())
            {
                var setting = context.Query<AppSetting>().FirstOrDefault(x => x.Key == key);
                if(setting == null)
                {
                    setting = new AppSetting(key, value);
                    context.Set<AppSetting>().Add(setting);
                }else
                {
                    setting.Value = value;
                }
                context.SaveChanges();
            }
        }

        private void Load()
        {
            _settings = new Dictionary<string, string>();
            using(var context = _contextFactory.Invoke())
            {
                var settings = context.Query<AppSetting>().Select(x => x).ToList();
                foreach(var setting in settings)
                {
                    _settings[setting.Key] = setting.Value;
                }
            }
        }
    }
}