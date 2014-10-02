#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Helpers;
using apcurium.MK.Common.Configuration.Impl;
using MK.Common.Configuration;

#endregion

namespace MK.Booking.IBS.Test
{
    public class FakeConfigurationManager : IConfigurationManager
    {
        private readonly Dictionary<string, string> _settings = new Dictionary<string, string>
        {
            {"IBS.WebServicesPassword", "test"},
            {"IBS.WebServicesUrl", "http://apcuriumibs:6928/XDS_IASPI.DLL/soap/"},
            {"IBS.WebServicesUserName", "taxi"}
        };

        public ServerTaxiHailSetting ServerData { get; private set; }

        public IDictionary<string, string> GetSettings()
        {
            return _settings;
        }

        public ClientPaymentSettings GetPaymentSettings()
        {
            throw new NotImplementedException();
        }
    }
}