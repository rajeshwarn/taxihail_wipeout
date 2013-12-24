#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

#endregion

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

        public T GetSetting<T>(string key, T defaultValue) where T : struct
        {
            var value = GetSetting(key);
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;
            var converter = TypeDescriptor.GetConverter(defaultValue);
            if (converter == null)
                throw new InvalidOperationException("Type " + typeof (T).Name + " has no type converter");
            try
            {
                var convertFromInvariantString = converter.ConvertFromInvariantString(value);
                if (convertFromInvariantString != null)
                    return (T) convertFromInvariantString;
            }
            catch
            {
                Trace.WriteLine("Could not convert setting " + key + " to " + typeof (T).Name);
            }
            return defaultValue;
        }

        public IDictionary<string, string> GetSettings()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AppSetting>().ToArray().ToDictionary(kv => kv.Key, kv => kv.Value);
            }
        }

        public void Reset()
        {
        }

        public ClientPaymentSettings GetPaymentSettings(bool force = true)
        {
            using (var context = _contextFactory.Invoke())
            {
                var settings = context.Set<ServerPaymentSettings>().Find(AppConstants.CompanyId);
                return settings ?? new ServerPaymentSettings();
            }
        }
    }
}