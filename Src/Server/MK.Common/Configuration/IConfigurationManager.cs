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
        IDictionary<string, string> GetSettings();
        ClientPaymentSettings GetPaymentSettings();
    }
}