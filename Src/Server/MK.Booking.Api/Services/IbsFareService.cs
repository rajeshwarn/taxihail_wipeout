#region

using System;
using System.Globalization;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Maps.Impl;
using apcurium.MK.Common.Configuration;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class IbsFareService : Service
    {
        private readonly IBookingWebServiceClient _bookingWebServiceClient;

        private readonly IConfigurationManager _configManager;

        public IbsFareService(IBookingWebServiceClient bookingWebServiceClient, IConfigurationManager configManager)
        {
            _bookingWebServiceClient = bookingWebServiceClient;
            _configManager = configManager;
        }

        public DirectionInfo Get(IbsFareRequest request)
        {
            // TODO: Adapt distance format
            var fare = _bookingWebServiceClient.GetFareEstimate(request.PickupLatitude, request.PickupLongitude,
                request.DropoffLatitude, request.DropoffLongitude);
            return fare.FareEstimate != null
                ? new DirectionInfo
                {
                    Distance = (int) (fare.Distance*1000),
                    Price = fare.FareEstimate,
                    FormattedDistance = FormatDistance((int) (fare.Distance*1000)),
                    FormattedPrice = FormatPrice(fare.FareEstimate)
                }
                : new DirectionInfo();
        }

        private string FormatPrice(double? price)
        {
            if (price.HasValue)
            {
                var culture = _configManager.GetSetting("PriceFormat");
                return string.Format(new CultureInfo(culture), "{0:C}", price);
            }
            return "";
        }

        private string FormatDistance(int? distance)
        {
            if (distance.HasValue)
            {
                //var format = _configManager.GetSetting("DistanceFormat").ToEnum<DistanceFormat>(true, DistanceFormat.Km);                                                           
                var format = _configManager.GetSetting("DistanceFormat", Directions.DistanceFormat.Km);

                if (format == Directions.DistanceFormat.Km)
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