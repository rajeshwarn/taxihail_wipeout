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

namespace apcurium.MK.Booking.Api.Services
{





    public class DirectionsService : RestServiceBase<DirectionsRequest>
    {



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
                }
            }
            
            return result;
        }

        //TODO: Must be in the settings
        private const double _callCost = 3.45;
        private const double _costPerKm = 1.70;
        private const double _maxDistance = 50;


        private double? GetPrice(int? distance)
        {

            double? price = null;
            try
            {
                if (distance.HasValue && (distance.Value > 0))
                {
                    double km = (distance.Value / 1000);

                    if (km < 5)
                    {
                        price = _callCost + (km * _costPerKm) + (((km * _costPerKm) + _callCost) * 0.2);
                    }
                    else if (km < _maxDistance)
                    {
                        price = _callCost + (km * _costPerKm) + (((km * _costPerKm) + _callCost) * 0.2) + 2;
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
