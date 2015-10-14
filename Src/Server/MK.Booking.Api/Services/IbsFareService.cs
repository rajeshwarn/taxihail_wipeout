#region

using System;
using System.Globalization;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Maps.Impl;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using System.Linq;

#endregion

namespace apcurium.MK.Booking.Api.Services 
{
    public class IbsFareService : Service
    {
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IServerSettings _serverSettings;
        private readonly IVehicleTypeDao _vehicleTypeDao;
        private readonly Resources.Resources _resources;
        public IbsFareService(IIBSServiceProvider ibsServiceProvider, IServerSettings serverSettings, IVehicleTypeDao vehicleTypeDao)
        {
            _ibsServiceProvider = ibsServiceProvider;
            _serverSettings = serverSettings;
            _vehicleTypeDao = vehicleTypeDao;
            _resources = new Resources.Resources(serverSettings);
        }

        public DirectionInfo Get(IbsFareRequest request)
        {
            var tripDurationInMinutes = (request.TripDurationInSeconds.HasValue ? (int?)TimeSpan.FromSeconds(request.TripDurationInSeconds.Value).TotalMinutes : null);

            var defaultVehiculeType = _vehicleTypeDao.GetAll().FirstOrDefault();

            var fare = _ibsServiceProvider.Booking().GetFareEstimate(
                request.PickupLatitude,
                request.PickupLongitude,
                request.DropoffLatitude,
                request.DropoffLongitude, 
                request.PickupZipCode, 
                request.DropoffZipCode,
                request.AccountNumber,
                request.CustomerNumber,
                tripDurationInMinutes, 
                _serverSettings.ServerData.DefaultBookingSettings.ProviderId,
                request.VehicleType,
                defaultVehiculeType != null ? defaultVehiculeType.ReferenceDataVehicleId : -1);

            if (fare.FareEstimate != null)
            {
                double distance = fare.Distance ?? 0;

                return new DirectionInfo
                {
                    Distance = distance,
                    Price = fare.FareEstimate,
                    FormattedDistance = _resources.FormatDistance(distance),
                    FormattedPrice = _resources.FormatPrice(fare.FareEstimate)
                };
            }

            return new DirectionInfo();
        }

        public DirectionInfo Get(IbsDistanceRequest request)
        {
            var tripDurationInMinutes = (request.WaitTime.HasValue ? (int?)TimeSpan.FromSeconds(request.WaitTime.Value).TotalMinutes : null);

            var defaultVehiculeType = _vehicleTypeDao.GetAll().FirstOrDefault();

            var fare = _ibsServiceProvider.Booking().GetDistanceEstimate(
                request.Distance,
                tripDurationInMinutes,
                request.StopCount,
                request.PassengerCount,
                request.VehicleType,
                defaultVehiculeType != null ? defaultVehiculeType.ReferenceDataVehicleId : -1,
                request.AccountNumber,
                request.CustomerNumber
                );

            if (fare.TotalFare != null)
            {
                double distance = request.Distance;

                return new DirectionInfo
                {
                    Distance = distance,
                    Price = fare.TotalFare,
                    FormattedDistance = _resources.FormatDistance(distance),
                    FormattedPrice = _resources.FormatPrice(fare.TotalFare)
                };
            }

            return new DirectionInfo();
        }
    }
}