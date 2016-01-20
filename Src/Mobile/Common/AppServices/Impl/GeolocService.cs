using System;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Common.Entity;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{

    public class GeolocService : BaseService, IGeolocService
    {
		private readonly IGeocoding _geocoding;
		private readonly IAddresses _addresses;
		private readonly IDirections _directions;
        private readonly INetworkRoamingService _networkRoamingService;

        public GeolocService(IGeocoding geocoding, IAddresses addresses, IDirections directions, INetworkRoamingService networkRoamingService)
		{
			_directions = directions;
            _networkRoamingService = networkRoamingService;
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
                var marketTariff = await GetMarketTariffIfPossible(originLat, originLong);

				var direction = await _directions.GetDirectionAsync(originLat, originLong, destLat, destLong, vehicleTypeId, date, false, marketTariff);
                return new DirectionInfo { Distance = direction.Distance, FormattedDistance = direction.FormattedDistance, Price = direction.Price, TripDurationInSeconds = (int?)direction.Duration };
            }
            catch
            {
                return new DirectionInfo();
            }
        }

        private async Task<Tariff> GetMarketTariffIfPossible(double latitude, double longitude)
        {
            var marketSettings = await _networkRoamingService.GetHashedCompanyMarket(latitude, longitude);
            
            // If we are in local market or the app fare estimates is not ovverridden in the roaming market we do not return a tariff override.
            if (!marketSettings.HashedMarket.HasValueTrimmed() || !marketSettings.OverrideEnableAppFareEstimates)
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