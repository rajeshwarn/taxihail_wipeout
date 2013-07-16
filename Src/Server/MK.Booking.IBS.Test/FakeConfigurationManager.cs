using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Configuration;

namespace MK.Booking.IBS.Test
{
    public class FakeConfigurationManager: IConfigurationManager
    {
        readonly IDictionary<string, string> _settings = new Dictionary<string, string>()
        {
            {"IBS.WebServicesPassword", "test"},
            {"IBS.WebServicesUrl", "http://72.38.252.190:6928/XDS_IASPI.DLL/soap/"},
            {"IBS.WebServicesUserName", "taxi"}
        };

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public string GetSetting(string key)
        {
            return _settings[key];
        }

        public T GetSetting<T>(string key, T defaultValue) where T : struct
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, string> GetSettings()
        {
            return _settings;
        }

        public void SetSettings(IDictionary<string, string> appSettings)
        {
            throw new NotImplementedException();
        }

        public ClientPaymentSettings GetPaymentSettings(bool cleanCache = false)
        {
            throw new NotImplementedException();
        }
    }
}
