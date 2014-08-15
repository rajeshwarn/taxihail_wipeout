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
		                                    double? destinationLng, DateTime? date = default(DateTime?), 
												bool forEta = false)
		{
			var result = new Direction ();
			var direction = _client.GetDirections (
                    originLat.GetValueOrDefault (), originLng.GetValueOrDefault (),
				    destinationLat.GetValueOrDefault (), destinationLng.GetValueOrDefault (),
                    date);

			direction = new GeoDirection ();

			direction.Distance = 4500;
			direction.Duration = 140;
			direction.TrafficDelay = 0;

			if (direction.Distance.HasValue) 
            {
                result.Duration = direction.Duration;
				result.Distance = direction.Distance;

				if (!forEta) 
				{
					result.Price = _priceCalculator.GetPrice (
						direction.Distance, 
						date ?? DateTime.Now, 
						direction.Duration);
				}

				result.FormattedDistance = FormatDistance (result.Distance);
			}

			return result;
		}

		private string FormatDistance (int? distance)
		{
			string result = "";

			if (distance.HasValue) {
				double meters = (double)distance.Value / 1000;

				switch (_appSettings.Data.DistanceFormat.ToEnum (true, DistanceFormat.Km)) {

				case DistanceFormat.Mile:
					double miles = Math.Round (meters / 1.609344, 1);
					result = string.Format ("{0:n1} mile" + ((miles == 1 || miles == 0) ? "" : "s"), miles);
					break;

				case DistanceFormat.Km:
				default:
					result = string.Format ("{0:n1} km", Math.Round (meters, 1));
					break;
				}
			}

			return result;
		}
	}
}