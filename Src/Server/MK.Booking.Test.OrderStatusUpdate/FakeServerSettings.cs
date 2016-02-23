using System;
using System.Collections.Generic;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Helpers;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Enumeration;

namespace MK.Booking.Test.OrderStatusUpdate
{
    public class FakeServerSettings : IServerSettings
    {
        public FakeServerSettings(bool useHoneyBadger = false)
        {
            ServerData = new ServerTaxiHailSetting();
            var config = new Dictionary<string, string>
            {
                {"TaxiHail.ApplicationName", "Apcurium" }
            };

            if (useHoneyBadger)
            {
                config.Add("LocalAvailableVehiclesMode", ((int)LocalAvailableVehiclesModes.HoneyBadger).ToString());
                config.Add("ExternalAvailableVehiclesMode", ((int)ExternalAvailableVehiclesModes.HoneyBadger).ToString());
                config.Add("HoneyBadger.AvailableVehiclesMarket", "MTL");
            }
            else
            {
                config.Add("LocalAvailableVehiclesMode", ((int)LocalAvailableVehiclesModes.Geo).ToString());
                config.Add("ExternalAvailableVehiclesMode", ((int)ExternalAvailableVehiclesModes.Geo).ToString());
                config.Add("HoneyBadger.AvailableVehiclesMarket", string.Empty);
            }
           
            SetSettingsValue(config);
        }

        private void SetSettingsValue(IDictionary<string, string> values)
        {
            SettingsLoader.InitializeDataObjects(ServerData, values, null);
        }
        public ServerTaxiHailSetting ServerData { get; private set; }
        public ServerPaymentSettings GetPaymentSettings(string companyKey = null)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, string> GetSettings()
        {
            throw new NotImplementedException();
        }

        public void Reload()
        {
            throw new NotImplementedException();
        }
    }
}
