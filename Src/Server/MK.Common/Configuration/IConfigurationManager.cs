#region

using System.Collections.Generic;
using apcurium.MK.Common.Configuration.Impl;
using MK.Common.Configuration;

#endregion

namespace apcurium.MK.Common.Configuration
{
    public interface IConfigurationManager
    {
        ServerTaxiHailSetting ServerData { get; }
        ClientPaymentSettings GetPaymentSettings();

        IDictionary<string, string> GetSettings();
        void Reload();
    }
}