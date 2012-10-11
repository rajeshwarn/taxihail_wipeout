using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Configuration;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Api.Services
{
    public class ConfigurationsService: RestServiceBase<ConfigurationsRequest>
    {

        private IConfigurationManager _configManager;
         

        public ConfigurationsService(IConfigurationManager configManager )
        {
            _configManager = configManager;
        }


        public override object OnGet(ConfigurationsRequest request)
        {
            var keys = new string[] { "PriceFormat", "DistanceFormat", "Direction.FlateRate", "Direction.RatePerKm", "Direction.MaxDistance", "GeoLoc.SearchFilter", "NearbyPlacesService.DefaultRadius", "Map.PlacesApiKey" };

            var allKeys = _configManager.GetAllSettings();

            return allKeys.Where(k => keys.Contains(k.Key)).ToArray();
            //return true;
        }

    }
}
