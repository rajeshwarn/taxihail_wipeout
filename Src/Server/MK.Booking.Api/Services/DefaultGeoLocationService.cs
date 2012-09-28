using System.Globalization;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Services
{
    public class DefaultGeoLocationService : RestServiceBase<DefaultGeoLocationRequest>
    {
        private readonly IConfigurationManager _configurationManager;

        public DefaultGeoLocationService(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        public override object OnGet(DefaultGeoLocationRequest request)
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