#region

using apcurium.MK.Booking.MapDataProvider;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using System;
using System.Globalization;


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

        private CultureInfo _currentCulture;
        public Directions(IDirectionDataProvider client, IAppSettings appSettings, IPriceCalculator priceCalculator)
		{
			_client = client;
			_appSettings = appSettings;
			_priceCalculator = priceCalculator;

            _currentCulture = new CultureInfo(_appSettings.Data.PriceFormat);
		}

		public Direction GetDirection (double? originLat, double? originLng, double? destinationLat,
                                            double? destinationLng, string currencyPriceFormat, int? vehicleTypeId = null, DateTime? date = default(DateTime?))
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
                    direction.Duration, vehicleTypeId);

                
                result.FormattedDistance = FormatDistance (result.Distance);
                result.FormattedPrice = result.Price == null ? string.Empty : FormatCurrency(result.Price.Value, currencyPriceFormat);
			}

			return result;
		}

		private string FormatDistance (int? distance)
		{
            if (distance.HasValue) 
            {
				var format = _appSettings.Data.DistanceFormat.ToEnum (true, DistanceFormat.Km);
                var distanceIsKm = format.Equals(DistanceFormat.Km);
                var distanceUnit = distanceIsKm ? "km" : "miles";
                var distanceInUnit = distanceIsKm ? Math.Round ((double)distance.Value / 1000, 1) : Math.Round ((double)distance.Value / 1000 / 1.609344, 1);
                var distanceStringFormat = "{0:n2} {1}";
                return string.Format(distanceStringFormat, distanceInUnit, distanceUnit);
			}
			return "";
		}


        public string FormatCurrency(double amount, string currencyPriceFormat)
        {
            var stringFormattedAsACurrency = string.Format(_currentCulture, currencyPriceFormat, amount);
            return stringFormattedAsACurrency.Replace(_currentCulture.NumberFormat.NumberDecimalSeparator, _currentCulture.NumberFormat.CurrencyDecimalSeparator);
        }
	}
}