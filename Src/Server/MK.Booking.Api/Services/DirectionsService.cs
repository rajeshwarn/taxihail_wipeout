using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Common.Extensions;
using ServiceStack.Common.Web;
using System.Net;
using System.Globalization;
using apcurium.MK.Booking.Api.Services.GoogleApi;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Services
{

    public enum DistanceFormat
    {
        Km,
        Mile,
    }



    public class DirectionsService : RestServiceBase<DirectionsRequest>
    {
        private IConfigurationManager _configManager;
        public DirectionsService(IConfigurationManager configManager)
        {
            _configManager = configManager;
        }


        public override object OnGet(DirectionsRequest request)
        {
            
            var result = new DirectionInfo();

            var client = new JsonServiceClient("http://maps.googleapis.com/maps/api/");

            var resource = string.Format(CultureInfo.InvariantCulture, "directions/json?origin={0},{1}&destination={2},{3}&sensor=false", request.OriginLat, request.OriginLng, request.DestinationLat, request.DestinationLng);            

            var directions = client.Get<DirectionResult>(resource);

            if (directions.Status == ResultStatus.OK) 
            {
                var route = directions.Routes.ElementAt(0);
                if ( route.Legs.Count > 0 )
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
                    double distanceInKM = Math.Round( (double)distance.Value / 1000, 1);
                    return string.Format("{0:n1} km", distanceInKM);
                }
                else
                {
                    double distanceInMiles = Math.Round((double)distance.Value / 1000, 1);
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
            double callCost = double.Parse(  _configManager.GetSetting("Direction.FlateRate"), CultureInfo.InvariantCulture );
            double costPerKm = double.Parse( _configManager.GetSetting("Direction.RatePerKm"), CultureInfo.InvariantCulture );
            double maxDistance = double.Parse( _configManager.GetSetting("Direction.MaxDistance"), CultureInfo.InvariantCulture );
            


            double? price = null;
            try
            {
                if (distance.HasValue && (distance.Value > 0))
                {
                    double km = (distance.Value / 1000);

                    if (km < 5)
                    {
                        price = callCost + (km * costPerKm) + (((km * costPerKm) + callCost) * 0.2);
                    }
                    else if (km < maxDistance)
                    {
                        price = callCost + (km * costPerKm) + (((km * costPerKm) + callCost) * 0.2) + 2;
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
