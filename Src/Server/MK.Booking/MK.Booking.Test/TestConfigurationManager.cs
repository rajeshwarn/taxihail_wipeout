#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Helpers;
using apcurium.MK.Common.Configuration.Impl;
using MK.Common.Configuration;
using Newtonsoft.Json.Linq;
using ServiceStack.Text;

#endregion

namespace apcurium.MK.Booking.Common.Tests
{
    public class TestConfigurationManager : IConfigurationManager, IAppSettings
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

            ServerData = new ServerTaxiHailSetting();
            
            SetSettingsValue(_config);
        }

        private void SetSettingsValue(IDictionary<string, string> values)
        {
            ConfigManagerLoader.InitializeDataObjects(ServerData, values, null);
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType
                && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
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

        public IDictionary<string, string> GetSettings()
        {
            return _config;
        }

        public ClientPaymentSettings GetPaymentSettings()
        {
            throw new NotImplementedException();
        }

        public void Reload()
        {
        }

        public void SetSetting(string key, string value)
        {
            _config[key] = value;
            ServerData = new ServerTaxiHailSetting();
            SetSettingsValue(_config);
        }

        public TaxiHailSetting Data { get { return ServerData; } }
        public ServerTaxiHailSetting ServerData { get; private set; }
        public void Load()
        {
           //done in the ctor
        }

        public void ChangeServerUrl(string serverUrl)
        {
            throw new NotImplementedException();
        }
    }
}