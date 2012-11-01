using System.Collections.Generic;

namespace apcurium.MK.Common.Configuration
{
    public interface IConfigurationManager
    {
        void Reset();
        string GetSetting( string key );
        //void SetSetting( string key, string value );

        IDictionary<string, string> GetSettings();
        void SetSettings(IDictionary<string, string> appSettings);
    }
}
