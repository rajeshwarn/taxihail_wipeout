using System.Net;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Configuration;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Api.Services
{
    public class ConfigurationsService : RestServiceBase<ConfigurationsRequest>
    {
        private readonly IConfigurationManager _configManager;
        private readonly ICommandBus _commandBus;

        public ConfigurationsService(IConfigurationManager configManager, ICommandBus commandBus)
        {
            _configManager = configManager;
            _commandBus = commandBus;
        }

        public override object OnGet(ConfigurationsRequest request)
        {
            string[] keys;

            if (request.AppSettingsType.Equals(AppSettingsType.Webapp))
            {
                keys = new[] { "PriceFormat", "DistanceFormat", "Direction.FlateRate", "Direction.RatePerKm",
                               "Direction.MaxDistance", "GeoLoc.SearchFilter", "GeoLoc.PopularAddress.Range",
                               "NearbyPlacesService.DefaultRadius", "Map.PlacesApiKey", "Client.HideCallDispatchButton",
                               "IBS.ExcludedVehicleTypeId", "IBS.ExcludedPaymentTypeId", "IBS.ExcludedProviderId"
                           };
            }
            else //AppSettingsType.Mobile
            {
                keys = new[] { "PriceFormat", "DistanceFormat", "Direction.FlateRate", "Direction.RatePerKm", "Direction.MaxDistance", 
                    "GeoLoc.SearchFilter", "GeoLoc.PopularAddress.Range", "NearbyPlacesService.DefaultRadius", "Map.PlacesApiKey", "Client.HideCallDispatchButton" };
            }

            var allKeys = _configManager.GetSettings();

            var result = allKeys.Where(k => keys.Contains(k.Key)).ToDictionary(s => s.Key, s =>  s.Value);

            return result;
        }

        public override object OnPost(ConfigurationsRequest request)
        {
            var command = new Commands.AddOrUpdateAppSettings { AppSettings = request.AppSettings };
            _commandBus.Send(command);

            return "";
        }
    }
}
