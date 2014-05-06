#region

using System;
using System.Globalization;
using System.Linq;


using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.MapDataProvider;
using apcurium.MK.Booking.MapDataProvider.Resources;

#endregion

namespace apcurium.MK.Booking.Maps.Impl
{
    public class Directions : IDirections
    {
        public enum DistanceFormat
        {
            Km,
            Mile,
        }


        private readonly IMapsApiClient _client;
        private readonly IAppSettings _appSettings;
        private readonly IPriceCalculator _priceCalculator;

        public Directions(IMapsApiClient client, IAppSettings appSettings, IPriceCalculator priceCalculator)
        {
            _client = client;
            _appSettings = appSettings;
            _priceCalculator = priceCalculator;
        }


        public Direction GetDirection(double? originLat, double? originLng, double? destinationLat,
            double? destinationLng, DateTime? date = default(DateTime?))
        {
            var result = new Direction();
            var directions = _client.GetDirections(originLat.GetValueOrDefault(), originLng.GetValueOrDefault(),
                destinationLat.GetValueOrDefault(), destinationLng.GetValueOrDefault());

            if (directions.Status == ResultStatus.OK)
            {
                var route = directions.Routes.ElementAt(0);
                if (route.Legs.Count > 0)
                {
                    var distance = route.Legs.Sum(leg => leg.Distance.Value);
                    var duration = route.Legs.Sum(leg => leg.Duration.Value);

                    result.Duration = duration;
                    result.Distance = distance;
                    result.Price = _priceCalculator.GetPrice(distance, date ?? DateTime.Now);

                    result.FormattedPrice = FormatPrice(result.Price);
                    result.FormattedDistance = FormatDistance(result.Distance);
                }
            }

            return result;
        }

        private string FormatPrice(double? price)
        {
            if (price.HasValue)
            {
                var culture = _appSettings.Data.PriceFormat;
                return string.Format(new CultureInfo(culture), "{0:C}", price);
            }
            return "";
        }

        private string FormatDistance(int? distance)
        {
            if (distance.HasValue)
            {
                var format = _appSettings.Data.DistanceFormat.ToEnum(true, DistanceFormat.Km);
                if (format == DistanceFormat.Km)
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