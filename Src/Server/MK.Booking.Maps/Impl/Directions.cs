#region

using System;
using System.Globalization;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.MapDataProvider;

#endregion
namespace apcurium.MK.Booking.Maps.Impl
{
    public class Directions : IDirections
    {
        private readonly IDirectionDataProvider _client;
        private readonly IAppSettings _appSettings;
        private readonly IPriceCalculator _priceCalculator;

        public Directions(IDirectionDataProvider client, IAppSettings appSettings, IPriceCalculator priceCalculator)
        {
            _client = client;
            _appSettings = appSettings;
            _priceCalculator = priceCalculator;
        }

        public Direction GetDirection(double? originLat, double? originLng, double? destinationLat,
		    double? destinationLng, int? vehicleTypeId = null, DateTime? date = default(DateTime?), bool forEta = false)
        {
            var result = new Direction();
            var direction = _client.GetDirections(
                    originLat.GetValueOrDefault(), originLng.GetValueOrDefault(),
                    destinationLat.GetValueOrDefault(), destinationLng.GetValueOrDefault(),
                    date);

            if (direction.Distance.HasValue)
            {
				if(direction.Duration.HasValue)
				{
					var paddedDuration = (int)Math.Ceiling((direction.Duration.Value * _appSettings.Data.EtaPaddingRatio) / 60);
					result.Duration = Math.Max (1, paddedDuration);
				}
                
                result.Distance = direction.Distance;

                if (!forEta)
                {
                    result.Price = _priceCalculator.GetPrice(
                        direction.Distance,
                        date ?? DateTime.Now,
                        direction.Duration, vehicleTypeId);

                    result.FormattedPrice = result.Price == null 
                        ? string.Empty 
                        : string.Format(new CultureInfo(_appSettings.Data.PriceFormat), "{0:C}", result.Price.Value);
                }

                result.FormattedDistance = FormatDistance(result.Distance);
            }

            return result;
        }

        public Direction GetEta(double fromLat, double fromLng, double toLat, double toLng)
        {
            return GetDirection(fromLat, fromLng, toLat, toLng, null, null, true);
        }

        private string FormatDistance(int? distance)
        {
            string result = "";

            if (distance.HasValue)
            {
                double meters = (double)distance.Value / 1000;

                switch (_appSettings.Data.DistanceFormat)
                {
                    case DistanceFormat.Mile:
                        double miles = Math.Round(meters / 1.609344, 1);
                        result = string.Format("{0:n1} mile" + ((miles == 1 || miles == 0) ? "" : "s"), miles);
                        break;
                    case DistanceFormat.Km:
                    default:
                        result = string.Format("{0:n1} km", Math.Round(meters, 1));
                        break;
                }
            }

            return result;
        }		

    }
}
