using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Common.Configuration
{
    public interface IConfigurationManager
    {
        string GetSetting( string key );
        void SetSetting( string key, string value );
    }
}
