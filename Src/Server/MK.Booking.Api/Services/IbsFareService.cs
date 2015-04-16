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

#endregion

namespace apcurium.MK.Booking.Api.Services 
{
    public class IbsFareService : Service
    {
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IServerSettings _serverSettings;
        private readonly Resources.Resources _resources;
        public IbsFareService(IIBSServiceProvider ibsServiceProvider, IServerSettings serverSettings)
        {
            _ibsServiceProvider = ibsServiceProvider;
            _serverSettings = serverSettings;
            _resources = new Resources.Resources(serverSettings);
        }

        public DirectionInfo Get(IbsFareRequest request)
        {
            var tripDurationInMinutes = (request.TripDurationInSeconds.HasValue ? (int?)TimeSpan.FromSeconds(request.TripDurationInSeconds.Value).TotalMinutes : null);

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
                request.VehicleType);

            if (fare.FareEstimate != null)
            {
                double distance = fare.Distance ?? 0;

                if (_serverSettings.ServerData.DistanceFormat == DistanceFormat.Km)
                {
                    // If IBS is not set in miles, it returns a distance in meters, so we have to convert it
                    distance = distance * 1000;
                }

                return new DirectionInfo
                {
                    Distance = distance,
                    Price = fare.FareEstimate,
                    FormattedDistance = FormatDistance(distance),
                    FormattedPrice = _resources.FormatPrice(fare.FareEstimate)
                };
            }

            return new DirectionInfo();
        }

        private string FormatDistance(double? distance)
        {
            if (distance.HasValue)
            {
                if (_serverSettings.ServerData.DistanceFormat == DistanceFormat.Km)
                {
                    var distanceInKm = Math.Round(distance.Value, 1);
                    return string.Format("{0:n1} km", distanceInKm);
                }

                return string.Format("{0:n1} miles", distance.Value);
            }

            return string.Empty;
        }
    }
}