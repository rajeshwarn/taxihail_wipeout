﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using ServiceStack.Common.Extensions;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services.Maps
{
    public class DirectionsService : Service
    {
        private readonly IDirections _client;
        private readonly IConfigurationManager _configManager;
        private readonly IBookingWebServiceClient _bookingWebServiceClient;
        private readonly IOrderDao _orderDao;

        public DirectionsService(IDirections client, IConfigurationManager configManager, IBookingWebServiceClient bookingWebServiceClient, IOrderDao orderDao)
        {
            _client = client;
            _configManager = configManager;
            _bookingWebServiceClient = bookingWebServiceClient;
            _orderDao = orderDao;
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

            if (_configManager.ServerData.ShowEta
                && request.OriginLat.HasValue
                && request.OriginLng.HasValue)
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
                                           request.OriginLat.Value,
                                           request.OriginLng.Value);

                    directionInfo.EtaFormattedDistance = etaDirectionInfo.FormattedDistance;
                    directionInfo.EtaDuration = etaDirectionInfo.Duration;
                }
            }

            return directionInfo;
        }

        public Direction Get(AssignedEtaRequest request)
        {
            var order = _orderDao.FindById(request.OrderId);
            return _client.GetEta(request.VehicleLat, request.VehicleLng, order.PickupAddress.Latitude, order.PickupAddress.Longitude);
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