using System;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Common.Entity;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{

    public class GeolocService : BaseService, IGeolocService
    {
		readonly IGeocoding _geocoding;
		readonly IAddresses _addresses;
		readonly IDirections _directions;

		public GeolocService(IGeocoding geocoding, IAddresses addresses, IDirections directions)
		{
			_directions = directions;
			_addresses = addresses;
			_geocoding = geocoding;
		}

        public async Task<Address> ValidateAddress(string address)
        {
            try
            {
				var currentLanguage = TinyIoCContainer.Current.Resolve<ILocalization> ().CurrentLanguage;
				var addresses = await _geocoding.SearchAsync(address, currentLanguage);
                
				return addresses.FirstOrDefault();
            }
            catch (Exception ex)
            {
				Logger.LogError (ex);
                return null;
            }
        }

        public async Task<Address[]> SearchAddress(double latitude, double longitude, bool searchPopularAddresses = false)
        {
            try
            {   
				var currentLanguage = TinyIoCContainer.Current.Resolve<ILocalization> ().CurrentLanguage;
                
                return await _geocoding.SearchAsync(latitude, longitude, currentLanguage, geoResult: null, searchPopularAddresses: searchPopularAddresses);
            }
            catch (Exception ex)
            {
				Logger.LogError(ex);
                return new Address[0];
            }
        }

        public Task<Address[]> SearchAddress(string address, double? latitude = null, double? longitude = null)
        {
            var currentLanguage = TinyIoCContainer.Current.Resolve<ILocalization>().CurrentLanguage;
            return _addresses.SearchAsync(address, latitude, longitude, currentLanguage);
        }


        public Task<DirectionInfo> GetDirectionInfo(Address origin, Address dest, int? vehicleTypeId = null)
        {
            if (origin.HasValidCoordinate() && dest.HasValidCoordinate())
            {
                return GetDirectionInfo(origin.Latitude, origin.Longitude, dest.Latitude, dest.Longitude, vehicleTypeId);
            }
			return Task.FromResult(new DirectionInfo());
        }

        public async Task<DirectionInfo> GetDirectionInfo(double originLat, double originLong, double destLat, double destLong, int? vehicleTypeId = null, DateTime? date = null)
        {
            try
            {
                var marketTariff = GetMarketTariffIfPossible(originLat, originLat);

				var direction = await _directions.GetDirectionAsync(originLat, originLong, destLat, destLong, vehicleTypeId, date, false, marketTariff);
                return new DirectionInfo { Distance = direction.Distance, FormattedDistance = direction.FormattedDistance, Price = direction.Price, TripDurationInSeconds = (int?)direction.Duration };
            }
            catch
            {
                return new DirectionInfo();
            }
        }

        private Tariff GetMarketTariffIfPossible(double latitude, double longitude)
        {
            //TODO MKTAXI-3799: place call to MarketSettings here

            var marketSettings = new MarketSettings()
            {
                KilometerIncluded = 3,
                OverrideEnableAppFareEstimates = true,
                MinimumRate = 2,
                MarginOfError = 10,
                PerMinuteRate = 3,
                FlatRate = 1,
                KilometricRate = 2,
                HashedMarket = "notahashedmarket",
            };
            
            if (marketSettings.HashedMarket == null || !marketSettings.OverrideEnableAppFareEstimates)
            {
                return null;
            }

            return new Tariff()
            {
                FlatRate = marketSettings.FlatRate,
                KilometricRate = marketSettings.KilometricRate,
                MinimumRate = marketSettings.MinimumRate,
                KilometerIncluded = marketSettings.KilometerIncluded,
                PerMinuteRate = marketSettings.PerMinuteRate,
                MarginOfError = marketSettings.MarginOfError,
                Type = (int)TariffType.Default
            };
        }
    }
}