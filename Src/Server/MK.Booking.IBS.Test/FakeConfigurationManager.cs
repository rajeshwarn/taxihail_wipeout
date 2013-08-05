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
        readonly Dictionary<string, string> _settings = new Dictionary<string, string>()
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
            string val = null;
            if (_settings.TryGetValue(key, out val))
            {
                return val;
            }
            else
            {
                return null;
            }

        }

        public void AddKey(string key, string val)
        {
            _settings.Add(key, val);
        }

        public IDictionary<string, string> GetSettings()
        {
            return _settings;
        }

        public void SetSettings(IDictionary<string, string> appSettings)
        {
            throw new NotImplementedException();
        }
    }
}
