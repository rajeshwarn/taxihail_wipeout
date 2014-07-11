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

		private readonly IDirectionDataProvider _client;
		private readonly IAppSettings _appSettings;
		private readonly IPriceCalculator _priceCalculator;

		public Directions (IDirectionDataProvider client, IAppSettings appSettings, IPriceCalculator priceCalculator)
		{
			_client = client;
			_appSettings = appSettings;
			_priceCalculator = priceCalculator;
		}

		public Direction GetDirection (double? originLat, double? originLng, double? destinationLat,
		                                    double? destinationLng, DateTime? date = default(DateTime?))
		{
			var result = new Direction ();
			var direction = _client.GetDirections (
                    originLat.GetValueOrDefault (), originLng.GetValueOrDefault (),
				    destinationLat.GetValueOrDefault (), destinationLng.GetValueOrDefault (),
                    date);

			if (direction.Distance.HasValue) 
            {
                result.Duration = direction.Duration;
				result.Distance = direction.Distance;

                result.Price = _priceCalculator.GetPrice (
                    direction.Distance, 
                    date ?? DateTime.Now, 
                    direction.Duration);

				result.FormattedPrice = FormatPrice (result.Price);
				result.FormattedDistance = FormatDistance (result.Distance);
			}

			return result;
		}

		private string FormatPrice (double? price)
		{
			if (price.HasValue) {
				var culture = _appSettings.Data.PriceFormat;
				return string.Format (new CultureInfo (culture), "{0:C}", price);
			}
			return "";
		}

		private string FormatDistance (int? distance)
		{
			if (distance.HasValue) {
				var format = _appSettings.Data.DistanceFormat.ToEnum (true, DistanceFormat.Km);
				if (format == DistanceFormat.Km) {
					var distanceInKm = Math.Round ((double)distance.Value / 1000, 1);
					return string.Format ("{0:n1} km", distanceInKm);
				}
				var distanceInMiles = Math.Round ((double)distance.Value / 1000 / 1.609344, 1);
				return string.Format ("{0:n1} miles", distanceInMiles);
			}
			return "";
		}
	}
}