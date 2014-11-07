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

        public IbsFareService(IIBSServiceProvider ibsServiceProvider, IServerSettings serverSettings)
        {
            _ibsServiceProvider = ibsServiceProvider;
            _serverSettings = serverSettings;
        }

        public DirectionInfo Get(IbsFareRequest request)
        {
            // TODO: Adapt distance format
            var fare = _ibsServiceProvider.Booking().GetFareEstimate(request.PickupLatitude, request.PickupLongitude,
                request.DropoffLatitude, request.DropoffLongitude, request.AccountNumber, request.CustomerNumber, request.WaitTime);
            return fare.FareEstimate != null
                ? new DirectionInfo
                {
                    Distance = (int) (fare.Distance*1000),
                    Price = fare.FareEstimate,
                    FormattedDistance = FormatDistance((int) (fare.Distance*1000)),                    
                }
                : new DirectionInfo();
        }

        private string FormatDistance(int? distance)
        {
            if (distance.HasValue)
            {
                if (_serverSettings.ServerData.DistanceFormat == DistanceFormat.Km)
                {
                    var distanceInKm = Math.Round((double) distance.Value/1000, 1);
                    return string.Format("{0:n1} km", distanceInKm);
                }
                var distanceInMiles = Math.Round((double) distance.Value/1000/1.609344, 1);
                return string.Format("{0:n1} miles", distanceInMiles);
            }
            return "";
        }
    }
}