using System;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{

    public class GeolocService : BaseService, IGeolocService
    {
		readonly IGeocoding _geocoding;
		readonly IAddresses _addresses;
		readonly IDirections _directions;
		readonly IAccountService _accountService;

		public GeolocService(IGeocoding geocoding, IAddresses addresses, IDirections directions, IAccountService accountService)
		{
			_accountService = accountService;
			_directions = directions;
			_addresses = addresses;
			_geocoding = geocoding;
		}

        public Address ValidateAddress(string address)
        {
            try
            {
				var addresses = _geocoding.Search(address);
                return addresses.FirstOrDefault();
            }
            catch (Exception ex)
            {
				Logger.LogError (ex);
                return null;
            }
        }

        public Address[] SearchAddress(double latitude, double longitude, bool searchPopularAddresses = false)
        {
            try
            {                
				var addresses = _geocoding.Search(latitude, longitude, geoResult: null, searchPopularAddresses: searchPopularAddresses);
                return addresses;
            }
            catch (Exception ex)
            {
				Logger.LogError(ex);
                return new Address[0];
            }
        }

        public Address[] SearchAddress(string address, double? latitude = null, double? longitude = null)
        {
            try
            {                
				var addresses = _addresses.Search(address, latitude, longitude);
                return addresses;
            }
            catch( Exception ex )
            {
				Logger.LogError (ex);
                return new Address[0];
            }

        }

		public Task<DirectionInfo> GetDirectionInfo(Address origin, Address dest)
        {
            if (origin.HasValidCoordinate() && dest.HasValidCoordinate())
            {
                return GetDirectionInfo(origin.Latitude, origin.Longitude, dest.Latitude, dest.Longitude);
            }
			return Task.FromResult(new DirectionInfo());
        }

		public async Task<DirectionInfo> GetDirectionInfo(double originLat, double originLong, double destLat, double destLong, DateTime? date = null)
        {

            try
            {
				var direction = await Task.Run(() => _directions
					.GetDirection(originLat, originLong, destLat, destLong, date));

                return new DirectionInfo { Distance = direction.Distance, FormattedDistance = direction.FormattedDistance, FormattedPrice = direction.FormattedPrice, Price = direction.Price };
            }
            catch
            {
                return new DirectionInfo();
            }

        }
    }
}