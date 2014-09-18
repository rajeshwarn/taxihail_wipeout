#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using apcurium.MK.Common.Diagnostic;
using MK.Common.Configuration;
using ServiceStack.Text;

#endregion

namespace apcurium.MK.Common.Configuration.Impl
{
    public class ConfigurationManager : IConfigurationManager, IAppSettings
    {
        private readonly Func<ConfigurationDbContext> _contextFactory;
        private readonly ILogger _logger;

        public ConfigurationManager(Func<ConfigurationDbContext> contextFactory, ILogger logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            Data = new TaxiHailSetting();
            Load();
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
                throw new InvalidOperationException("Type " + typeof(T).Name + " has no type converter");
            try
            {
                var convertFromInvariantString = converter.ConvertFromInvariantString(value);
                if (convertFromInvariantString != null)
                    return (T)convertFromInvariantString;
            }
            catch
            {
                Trace.WriteLine("Could not convert setting " + key + " to " + typeof(T).Name);
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

        public TaxiHailSetting Data { get; private set; }
        public void Load()
        {
            SetSettingsValue(GetSettings());
        }

        void SetSettingsValue(IDictionary<string, string> values)
        {
            foreach (KeyValuePair<string, string> item in values)
            {
                try
                {
                    var typeOfSettings = typeof(TaxiHailSetting);

                    var propertyName = item.Key.Contains(".")
                        ? item.Key.SplitOnLast('.')[1]
                        : item.Key;

                    var propertyType = typeOfSettings.GetProperty(propertyName);

                    if ( propertyType == null )
                    {
                        Console.WriteLine("Warning - can't set value for property {0}, value was {1} - property not found", item.Key, item.Value);
                        continue;
                    }


                    var targetType = IsNullableType(propertyType.PropertyType)
                            ? Nullable.GetUnderlyingType(propertyType.PropertyType)
                            : propertyType.PropertyType;

                    if (targetType.IsEnum)
                    {
                        var propertyVal = Enum.Parse(targetType, item.Value);
                        propertyType.SetValue(Data, propertyVal);
                    }
                    else if ( IsNullableType(propertyType.PropertyType) && string.IsNullOrEmpty( item.Value ) )
                    {
                        propertyType.SetValue(Data, null);
                    }
                    else
                    {
                        var propertyVal = Convert.ChangeType(item.Value, targetType);
                        propertyType.SetValue(Data, propertyVal);
                    }




                }
                catch (Exception e)
                {
                    Console.WriteLine("Warning - can't set value for property {0}, value was {1}", item.Key, item.Value);
                    _logger.LogMessage("Warning - can't set value for property {0}, value was {1}", item.Key, item.Value);
                    _logger.LogError(e);
                }
            }
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType
                && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }



        public void ChangeServerUrl(string serverUrl)
        {
            throw new NotImplementedException();
        }


    }
}