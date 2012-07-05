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
            config.Add("IBS.WebServicesUrl", "http://72.38.252.190:6928//XDS_IASPI.DLL/soap/");

            config.Add("GeoLoc.SearchFilter", "{0},montreal,qc,canada&region=ca");
            config.Add("GeoLoc.AddressFilter", "canada");


            config.Add("Direction.FlateRate", "3.45");
            config.Add("Direction.RatePerKm", "1.70");
            config.Add("Direction.MaxDistance", "50");

            return config[key];
            
        }

        public void SetSetting(string key, string value)
        {
            
        }
    }
}
