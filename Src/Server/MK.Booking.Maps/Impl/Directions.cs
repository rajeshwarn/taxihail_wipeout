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
        
        public Directions(IMapsApiClient client, IConfigurationManager configManager)
        {
            _client = client;
            _configManager = configManager;
        }


        public Direction GetDirection(double? originLat, double? originLng, double? destinationLat, double? destinationLng)
        {
            var result = new Direction();
            var directions = _client.GetDirections(originLat.GetValueOrDefault(), originLng.GetValueOrDefault(), destinationLat.GetValueOrDefault(), destinationLng.GetValueOrDefault());

            if (directions.Status == Google.Resources.ResultStatus.OK)
            {
                var route = directions.Routes.ElementAt(0);
                if (route.Legs.Count > 0)
                {
                    var distance = route.Legs.Sum(leg => leg.Distance.Value);
                    
                    result.Distance = distance;                                        
                    result.Price = GetPrice(distance);

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


        private double? GetPrice(int? distance)
        {
            double callCost = double.Parse(_configManager.GetSetting("Direction.FlateRate"), CultureInfo.InvariantCulture);
            double costPerKm = double.Parse(_configManager.GetSetting("Direction.RatePerKm"), CultureInfo.InvariantCulture);
            double maxDistance = double.Parse(_configManager.GetSetting("Direction.MaxDistance"), CultureInfo.InvariantCulture);



            double? price = null;
            try
            {
                if (distance.HasValue && (distance.Value > 0))
                {
                    double km = ((double)distance.Value / 1000);

                    if (km < maxDistance)
                    {
                        price = callCost + (km * costPerKm) + (((km * costPerKm) + callCost) * 0.2);
                    }
                    else 
                    {
                        price = 1000;
                    }

                    if (price.HasValue)
                    {

                        price = Math.Round(price.Value, 2);

                        Console.WriteLine(price);

                        price = price.Value * 100;

                        int q = (int)(price.Value / 5.0);

                        int r;

                        Math.DivRem((int)price.Value, 5, out r);
                        Console.WriteLine(" r : " + r.ToString());
                        if (r > 0)
                        {
                            q++;
                        }

                        price = q * 5;

                        Console.WriteLine(price);

                        price = price.Value / 100;

                        Console.WriteLine(price);

                    }
                }
            }
            catch
            {
            }

            return price;
        }
    }
}
