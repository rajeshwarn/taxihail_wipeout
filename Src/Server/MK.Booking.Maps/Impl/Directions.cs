using apcurium.MK.Booking.Google;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private readonly IConfigurationManager _configManager;
        private readonly IPriceCalculator _priceCalculator;

        public Directions(IMapsApiClient client, IConfigurationManager configManager, IPriceCalculator priceCalculator)
        {
            _client = client;
            _configManager = configManager;
            _priceCalculator = priceCalculator;
        }


        public Direction GetDirection(double? originLat, double? originLng, double? destinationLat, double? destinationLng, DateTime? date = default(DateTime?))
        {
            var result = new Direction();
            var directions = _client.GetDirections(originLat.GetValueOrDefault(), originLng.GetValueOrDefault(), destinationLat.GetValueOrDefault(), destinationLng.GetValueOrDefault());

            if (directions.Status == Google.Resources.ResultStatus.OK)
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
                var culture = _configManager.GetSetting("PriceFormat");
                return string.Format(new CultureInfo(culture), "{0:C}", price);
            }
            else
            {
                return "";
            }

        }
        private string FormatDistance(int? distance)
        {
            if (distance.HasValue)
            {
                var format = _configManager.GetSetting("DistanceFormat").ToEnum<DistanceFormat>(true, DistanceFormat.Km);                
                if (format == DistanceFormat.Km)
                {
                    double distanceInKM = Math.Round((double)distance.Value / 1000, 1);
                    return string.Format("{0:n1} km", distanceInKM);
                }
                else
                {

                    double distanceInMiles = Math.Round((double)distance.Value / 1000 / 1.609344, 1);
                    
                    //format == DistanceFormat.Km ? distance : Convert.ToInt32(Math.Round(distance / 1609.344, 0));

                    return string.Format("{0:n1} miles", distanceInMiles);
                }
            }
            else
            {
                return "";
            }

        }
        
    }
}
