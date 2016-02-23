using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Helpers;
using apcurium.MK.Common.Configuration.Impl;
using Newtonsoft.Json.Linq;


namespace apcurium.MK.Booking.Test
{
    public class TestServerSettings : IServerSettings, IAppSettings
    {
        private readonly Dictionary<string, string> _config;
        private Dictionary<string, ServerPaymentSettings> _serverPaymentSettings;

        public TestServerSettings()
        {
            _config = new Dictionary<string, string>();
            _serverPaymentSettings = new Dictionary<string, ServerPaymentSettings>
            {
                { string.Empty, new ServerPaymentSettings() }
            };

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
            SettingsLoader.InitializeDataObjects(ServerData, values, null);
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

        public ServerPaymentSettings GetPaymentSettings(string companyKey = null)
        {
            return _serverPaymentSettings[companyKey ?? string.Empty];
        }

        public IDictionary<string, string> GetSettings()
        {
            return _config;
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
        public Task Load()
        {
            //done in the ctor
            return Task.FromResult(true);
        }

	public string GetServiceUrl()
	{
	    throw new NotImplementedException();
	}

        public Task ChangeServerUrl(string serverUrl)
        {
            throw new NotImplementedException();
        }

	    public void SetAppleTestAccountMode(bool isAppleTestAccountUsed)
	    {
		    throw new NotImplementedException();
	    }

	    public void SetPaymentSettings(string companyKey, ServerPaymentSettings settings)
        {
            companyKey = companyKey ?? string.Empty;
            if (_serverPaymentSettings.ContainsKey(companyKey))
            {
                _serverPaymentSettings[companyKey] = settings;
            }
            else
            {
                _serverPaymentSettings.Add(companyKey, settings);
            }
        }
    }
}
