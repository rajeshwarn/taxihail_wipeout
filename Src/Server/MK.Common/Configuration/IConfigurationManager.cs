using System.Collections.Generic;

namespace apcurium.MK.Common.Configuration
{
    public interface IConfigurationManager
    {
        void Reset();
        string GetSetting( string key );
        T GetSetting<T>(string key, T defaultValue) where T : struct; 
        
        IDictionary<string, string> GetSettings();
        void SetSettings(IDictionary<string, string> appSettings);


		ClientPaymentSettings GetPaymentSettings(bool cleanCache = false);

    }

}
