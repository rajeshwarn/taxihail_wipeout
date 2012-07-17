using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Common.Tests
{
    public class TestConfigurationManager : IConfigurationManager
    {
        private Dictionary<string, string> _config;

        public TestConfigurationManager()
        {
            _config = new Dictionary<string, string>();
            _config.Add("IBS.DefaultAccountPassword", "testpassword");
            _config.Add("IBS.WebServicesUserName", "taxi");
            _config.Add("IBS.WebServicesPassword", "test");
            _config.Add("IBS.WebServicesUrl", "http://72.38.252.190:6928/XDS_IASPI.DLL/soap/");

            _config.Add("GeoLoc.SearchFilter", "{0},montreal,qc,canada&region=ca");
            _config.Add("GeoLoc.AddressFilter", "canada");


            _config.Add("Direction.FlateRate", "3.45");
            _config.Add("Direction.RatePerKm", "1.70");
            _config.Add("Direction.MaxDistance", "50");

            _config.Add("Email.NoReply", "noreply@apcurium.com");

            _config.Add("DefaultBookingSettings.ChargeTypeId", "1");
            _config.Add("DefaultBookingSettings.NbPassenger", "2");
            _config.Add("DefaultBookingSettings.VehicleTypeId", "1");
            _config.Add("DefaultBookingSettings.ProviderId", "13");

            _config.Add("DistanceFormat", "KM"); // Other option is "MILE"
            _config.Add("PriceFormat", "en-US");

        }
        
        public string GetSetting(string key)
        {

            return _config[key];
            
        }

        public void SetSetting(string key, string value)
        {
            _config[key] = value;
        }
    }
}
