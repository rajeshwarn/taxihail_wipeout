#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
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
            ServerData = new ServerTaxiHailSetting();
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
        public ServerTaxiHailSetting ServerData { get; private set; }
        public void Load()
        {
            var dbSettings = GetSettings();
            SetSettingsValue(dbSettings);
            SetServerSettingsValue(dbSettings);
        }

        public void ChangeServerUrl(string serverUrl)
        {
            throw new NotImplementedException();
        }

        private void SetSettingsValue(IDictionary<string, string> values)
        {
            InitializeDataObjects(Data, values);
        }

        private void SetServerSettingsValue(IDictionary<string, string> values)
        {
            InitializeDataObjects(ServerData, values);
        }

        private void InitializeDataObjects<T>(T objectToInitialize, IDictionary<string, string> values) where T : class
        {
            var typeOfSettings = typeof(T);
            foreach (KeyValuePair<string, string> item in values)
            {
                try
                {
                    var propertyName = item.Key;
                    if (propertyName.Contains('.'))
                    {
                        if (propertyName.SplitOnFirst('.')[0] == "Client")
                        {
                            propertyName = propertyName.SplitOnFirst('.')[1];
                        }
                    }

                    var propertyType = GetProperty(typeOfSettings, propertyName);
                    
                    if (propertyType == null)
                    {
                        Console.WriteLine("Warning - can't set value for property {0}, value was {1} - property not found", propertyName, item.Value);
                        continue;
                    }

                    var targetType = IsNullableType(propertyType.PropertyType)
                        ? Nullable.GetUnderlyingType(propertyType.PropertyType)
                        : propertyType.PropertyType;

                    if (targetType.IsEnum)
                    {
                        var propertyVal = Enum.Parse(targetType, item.Value);
                        SetValue(propertyName, objectToInitialize, propertyVal);
                    }
                    else if (IsNullableType(propertyType.PropertyType) && string.IsNullOrEmpty(item.Value))
                    {
                        SetValue(propertyName, objectToInitialize, null);
                    }
                    else
                    {
                        var propertyVal = Convert.ChangeType(item.Value, targetType);
                        SetValue(propertyName, objectToInitialize, propertyVal);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Warning - can't set value for property {0}, value was {1}", item.Key, item.Value);
                    _logger.Maybe(() => _logger.LogMessage("Warning - can't set value for property {0}, value was {1}", item.Key, item.Value));
//                    _logger.Maybe(() => _logger.LogError(e));
                }
            }
        }

        private static void SetValue(string compoundProperty, object target, object value)
        {
            string[] bits = compoundProperty.Split('.');
            for (int i = 0; i < bits.Length - 1; i++)
            {
                var propertyToGet = target.GetType().GetProperty(bits[i]);
                target = propertyToGet.GetValue(target, null);
            }
            var propertyToSet = target.GetType().GetProperty(bits.Last());
            propertyToSet.SetValue(target, value, null);
        }

        private static PropertyInfo GetProperty(Type type, string propertyName)
        {
            if (type.GetProperties().Count(p => p.Name == propertyName.Split('.')[0]) == 0)
            {
                throw new ArgumentNullException(string.Format("Property {0}, does not exist in type {1}", propertyName, type));
            }

            if (propertyName.Split('.').Length == 1)
            {
                return type.GetProperty(propertyName);
            }
            
            var firstProperty = type.GetProperty(propertyName.Split('.')[0]);
            if (firstProperty == null)
            {
                return null;
            }
            return GetProperty(firstProperty.PropertyType, propertyName.Split('.')[1]);
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType
                && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }
    }
}