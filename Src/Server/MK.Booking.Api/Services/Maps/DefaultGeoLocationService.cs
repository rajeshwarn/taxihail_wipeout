#region

using System.Globalization;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services.Maps
{
    public class DefaultGeoLocationService : Service
    {
        private readonly IConfigurationManager _configurationManager;

        public DefaultGeoLocationService(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        public object Get(DefaultGeoLocationRequest request)
        {
            var address = new Address
            {
                Latitude =
                    double.Parse(_configurationManager.GetSetting("GeoLoc.DefaultLatitude"),
                        CultureInfo.InvariantCulture),
                Longitude =
                    double.Parse(_configurationManager.GetSetting("GeoLoc.DefaultLongitude"),
                        CultureInfo.InvariantCulture)
            };

            return address;
        }
    }
}