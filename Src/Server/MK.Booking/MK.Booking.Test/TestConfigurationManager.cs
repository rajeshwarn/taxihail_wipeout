using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Common.Tests
{
    public class TestConfigurationManager : IConfigurationManager
    {
        private Dictionary<string, string> _config;

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

        public void Reset()
        { }

        public IDictionary<string, string> GetSettings()
        {

            return _config;

        }

        public void SetSettings(IDictionary<string, string> appSettings)
        {
            foreach (var appSetting in appSettings)
            {
                SetSetting(appSetting.Key, appSetting.Value);
            }
        }

        public string GetSetting(string key)
        {
            return _config[key];
        }

        public void SetSetting(string key, string value)
        {
            _config[key] = value;
        }

        static public string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
