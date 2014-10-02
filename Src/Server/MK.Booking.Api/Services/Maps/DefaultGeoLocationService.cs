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
            return new Address
            {
                Latitude = _configurationManager.ServerData.GeoLoc.DefaultLatitude,
                Longitude = _configurationManager.ServerData.GeoLoc.DefaultLongitude
            };
        }
    }
}