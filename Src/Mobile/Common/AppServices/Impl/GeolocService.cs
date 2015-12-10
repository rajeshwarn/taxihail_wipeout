using System;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Common.Entity;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Enumeration;

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


		public Task<DirectionInfo> GetDirectionInfo(Address origin, Address dest, ServiceType serviceType = ServiceType.Taxi, int? vehicleTypeId = null)
        {
            if (origin.HasValidCoordinate() && dest.HasValidCoordinate())
            {
				return GetDirectionInfo(origin.Latitude, origin.Longitude, dest.Latitude, dest.Longitude, serviceType, vehicleTypeId);
            }
			return Task.FromResult(new DirectionInfo());
        }

        public async Task<DirectionInfo> GetDirectionInfo(double originLat, double originLong, double destLat, double destLong, ServiceType serviceType = ServiceType.Taxi, int? vehicleTypeId = null, DateTime? date = null)
        {
            try
            {
				var direction = await _directions.GetDirectionAsync(originLat, originLong, destLat, destLong, serviceType, vehicleTypeId, date);
				return new DirectionInfo { Distance = direction.Distance, FormattedDistance = direction.FormattedDistance, Price = direction.Price };
            }
            catch
            {
                return new DirectionInfo();
            }
        }
    }
}