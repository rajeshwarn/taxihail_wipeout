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
        private readonly IServerSettings _serverSettings;
        private readonly IOrderDao _orderDao;
        private readonly IVehicleClient _vehicleClient;

        public DirectionsService(IDirections client, IServerSettings serverSettings, IOrderDao orderDao, IVehicleClient vehicleClient)
        {
            _client = client;
            _serverSettings = serverSettings;
            _orderDao = orderDao;
            _vehicleClient = vehicleClient;
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

            if (_serverSettings.ServerData.ShowEta
                && request.OriginLat.HasValue
                && request.OriginLng.HasValue)
            {
                // Get available vehicles

                var market = !string.IsNullOrWhiteSpace(request.Market) ? request.Market : string.Empty;
                
                var availableVehiclesTask = _vehicleClient.GetAvailableVehiclesAsync(request.OriginLat.Value,
                    request.OriginLng.Value, null, market);
                
                availableVehiclesTask.Wait();
                var availableVehicles = availableVehiclesTask.Result;

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

        private AvailableVehicle GetNearestAvailableVehicle(double originLat, double originLng, AvailableVehicle[] availableVehicles)
        {
            if (availableVehicles == null || !availableVehicles.Any())
            {
                return null;
            }

            return availableVehicles
                .OrderBy(car => Position.CalculateDistance(car.Latitude, car.Longitude, originLat, originLng))
                .ToArray().First();
        }
    }
}