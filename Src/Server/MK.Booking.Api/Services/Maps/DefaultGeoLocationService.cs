#region

using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Api.Services.Maps
{
    public class DefaultGeoLocationService : BaseApiService
    {
        private readonly IServerSettings _serverSettings;

        public DefaultGeoLocationService(IServerSettings serverSettings)
        {
            _serverSettings = serverSettings;
        }

        public Address Get()
        {
            return new Address
            {
                Latitude = _serverSettings.ServerData.GeoLoc.DefaultLatitude,
                Longitude = _serverSettings.ServerData.GeoLoc.DefaultLongitude
            };
        }
    }
}