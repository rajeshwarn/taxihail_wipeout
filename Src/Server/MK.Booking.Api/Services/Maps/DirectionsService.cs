#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Common.Configuration;
using ServiceStack.Common.Extensions;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services.Maps
{
    public class DirectionsService : Service
    {
        private readonly IDirections _client;
        private readonly IAppSettings _appSettings;
        private readonly IBookingWebServiceClient _bookingWebServiceClient;

        public DirectionsService(IDirections client, IAppSettings appSettings, IBookingWebServiceClient bookingWebServiceClient)
        {
            _client = client;
            _appSettings = appSettings;
            _bookingWebServiceClient = bookingWebServiceClient;
        }

        public object Get(DirectionsRequest request)
        {
            var result = _client.GetDirection(request.OriginLat, request.OriginLng, request.DestinationLat,
                request.DestinationLng, request.VehicleTypeId, request.Date);

            var directionInfo = new DirectionInfo
            {
                Distance = result.Distance,
                FormattedDistance = result.FormattedDistance,
                Price = result.Price,
                FormattedPrice = result.FormattedPrice
            };

            if (_appSettings.Data.ShowEta
                && request.OriginLat.HasValue
                && request.OriginLng.HasValue
                && request.DestinationLat.HasValue
                && request.DestinationLng.HasValue)
            {
                // Get available vehicles
                var availableVehicles = _bookingWebServiceClient.GetAvailableVehicles(request.OriginLat.Value, request.OriginLng.Value, null);

                // Get nearest available vehicle
                var nearestAvailableVehicle = GetNearestAvailableVehicle(request.OriginLat.Value,
                                                                         request.OriginLng.Value, 
                                                                         availableVehicles);
                
                if (nearestAvailableVehicle != null)
                {
                    // Get eta
                    var etaDirectionInfo = 
                            _client.GetEta(nearestAvailableVehicle.Latitude,
                                           nearestAvailableVehicle.Longitude,
                                           request.DestinationLat.Value,
                                           request.DestinationLng.Value);

                    directionInfo.EtaFormattedDistance = etaDirectionInfo.FormattedDistance;
                    directionInfo.EtaDuration = etaDirectionInfo.Duration;
                }
            }

            return directionInfo;
        }

        private IbsVehiclePosition GetNearestAvailableVehicle(double originLat, double originLng, IbsVehiclePosition[] avaiableVehicles)
        {
            if (avaiableVehicles == null || !avaiableVehicles.Any())
            {
                return null;
            }

            return avaiableVehicles
                .OrderBy(car => Position.CalculateDistance(car.Latitude, car.Longitude, originLat, originLng))
                .ToArray().First();
        }
    }
}