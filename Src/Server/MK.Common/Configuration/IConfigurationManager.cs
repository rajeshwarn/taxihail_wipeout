using System.Collections.Generic;

namespace apcurium.MK.Common.Configuration
{
    public interface IConfigurationManager
    {
        void Reset();
        string GetSetting( string key );
        T GetSetting<T>(string key, T defaultValue) where T : struct; 
        IDictionary<string, string> GetSettings();

		ClientPaymentSettings GetPaymentSettings(bool cleanCache = false);

    }

}
