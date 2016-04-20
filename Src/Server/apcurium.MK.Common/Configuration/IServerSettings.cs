using System.Collections.Generic;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Common.Configuration
{
    public interface IServerSettings
    {
        ServerTaxiHailSetting ServerData { get; }
        ServerPaymentSettings GetPaymentSettings(string companyKey = null);

        IDictionary<string, string> GetSettings();
        void Reload();
    }
}