using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Common.Diagnostic;
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
				string currentLanguage = TinyIoCContainer.Current.Resolve<ILocalization> ().CurrentLanguage;
				var addresses = _geocoding.Search(address, currentLanguage);
                
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
				string currentLanguage = TinyIoCContainer.Current.Resolve<ILocalization> ().CurrentLanguage;
				var addresses = _geocoding.Search(latitude, longitude, currentLanguage, geoResult: null, searchPopularAddresses: searchPopularAddresses);
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
				string currentLanguage = TinyIoCContainer.Current.Resolve<ILocalization> ().CurrentLanguage;
				var addresses = _addresses.Search(address, latitude, longitude, currentLanguage);
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
				var direction = await Task.Run(() => _directions.GetDirection(originLat, originLong, destLat, destLong, date));

                return new DirectionInfo { Distance = direction.Distance, FormattedDistance = direction.FormattedDistance, Price = direction.Price };
            }
            catch
            {
                return new DirectionInfo();
            }
        }
    }
}