#region

using System.Collections.Generic;
using apcurium.MK.Common.Configuration.Impl;

#endregion

namespace apcurium.MK.Common.Configuration
{
    public interface IConfigurationManager
    {
        void Reset();
        string GetSetting(string key);
        T GetSetting<T>(string key, T defaultValue) where T : struct;
        IDictionary<string, string> GetSettings();
        ClientPaymentSettings GetPaymentSettings(bool cleanCache = false);
    }
}