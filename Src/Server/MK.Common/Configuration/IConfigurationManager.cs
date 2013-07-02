using System.Collections.Generic;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Common.Configuration
{
    public interface IConfigurationManager
    {
        void Reset();
        string GetSetting( string key );
        
        IDictionary<string, string> GetSettings();
        void SetSettings(IDictionary<string, string> appSettings);

        PaymentSetting GetPaymentSettings();
    }
}
