﻿#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using apcurium.MK.Common.Configuration.Helpers;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using MK.Common.Configuration;
using ServiceStack.Messaging.Rcon;
using ServiceStack.Text;

#endregion

namespace apcurium.MK.Common.Configuration.Impl
{
    public class ServerSettings : IServerSettings, IAppSettings
    {
        private readonly Func<ConfigurationDbContext> _contextFactory;
        private readonly ILogger _logger;

        public ServerSettings(Func<ConfigurationDbContext> contextFactory, ILogger logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            ServerData = new ServerTaxiHailSetting();
            Load();
        }
        
        public IDictionary<string, string> GetSettings()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AppSetting>().ToArray().ToDictionary(kv => kv.Key, kv => kv.Value);
            }
        }

        public ServerPaymentSettings GetPaymentSettings(string companyKey = null)
        {
            using (var context = _contextFactory.Invoke())
            {
                var paymentSettings = companyKey.HasValue()
                    ? context.Set<ServerPaymentSettings>().FirstOrDefault(p => p.CompanyKey == companyKey)
                    : context.Set<ServerPaymentSettings>().Find(AppConstants.CompanyId);

                return paymentSettings ?? new ServerPaymentSettings();
            }
        }

        public void Reload()
        {
            ServerData = new ServerTaxiHailSetting();
            Load();
        }

        public TaxiHailSetting Data { get { return ServerData; } }
        public ServerTaxiHailSetting ServerData { get; private set; }

        public Task Load()
        {
            SettingsLoader.InitializeDataObjects(ServerData, GetSettings(), _logger);
            return Task.FromResult(true);
        }

        public Task ChangeServerUrl(string serverUrl)
        {
            throw new NotImplementedException();
        }
    }
}