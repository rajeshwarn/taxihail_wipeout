using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Common.Tests
{
    public class TestConfigurationManager : IConfigurationManager
    {
        
        public string GetSetting(string key)
        {
            var config = new Dictionary<string,string>();
            config.Add( "IBS.DefaultAccountPassword", "testpassword" );
            config.Add("IBS.WebServicesUserName", "taxi");
            config.Add("IBS.WebServicesPassword", "test");
            config.Add("IBS.WebServicesUrl", "http://drivelinq.dyndns-ip.com:6928/XDS_IASPI.DLL/soap/");

            return config[key];
            
        }

        public void SetSetting(string key, string value)
        {
            
        }
    }
}
