#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using Newtonsoft.Json.Linq;

#endregion

namespace apcurium.MK.Booking.Common.Tests
{
    public class TestConfigurationManager : IConfigurationManager
    {
        private readonly Dictionary<string, string> _config;

        public TestConfigurationManager()
        {
            _config = new Dictionary<string, string>();

            var jsonSettings = File.ReadAllText(Path.Combine(AssemblyDirectory, "MKWebDev.json"));
            var objectSettings = JObject.Parse(jsonSettings);
            foreach (var token in objectSettings)
            {
                _config.Add(token.Key, token.Value.ToString());
            }
        }

        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public void Reset()
        {
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
                return (T) converter.ConvertFromInvariantString(value);
            }
            catch
            {
                Trace.WriteLine("Could not convert setting " + key + " to " + typeof (T).Name);
            }
            return defaultValue;
        }

        public IDictionary<string, string> GetSettings()
        {
            return _config;
        }

        public ClientPaymentSettings GetPaymentSettings(bool force = true)
        {
            throw new NotImplementedException();
        }

        public string GetSetting(string key)
        {
            return _config.ContainsKey(key) ? _config[key] : null;
        }

        public void SetSetting(string key, string value)
        {
            _config[key] = value;
        }
    }
}