using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Common.Diagnostic;

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

        public DirectionInfo GetDirectionInfo(Address origin, Address dest)
        {
            if (origin.HasValidCoordinate() && dest.HasValidCoordinate())
            {
                return GetDirectionInfo(origin.Latitude, origin.Longitude, dest.Latitude, dest.Longitude);
            }
            return new DirectionInfo();
        }

        public DirectionInfo GetDirectionInfo(double originLat, double originLong, double destLat, double destLong, DateTime? date = null)
        {

            try
            {
				var direction = _directions.GetDirection(originLat, originLong, destLat, destLong, date);
                return new DirectionInfo { Distance = direction.Distance, FormattedDistance = direction.FormattedDistance, FormattedPrice = direction.FormattedPrice, Price = direction.Price };
            }
            catch
            {
                return new DirectionInfo();
            }

        }


        public IEnumerable<Address> FindSimilar(string address)
        {
            var addresses = new List<Address>();

            if (address.IsNullOrEmpty())
            {
                return addresses.ToArray();

            }

			var favAddresses = _accountService.GetFavoriteAddresses();
            var items = favAddresses as Address[] ?? favAddresses.ToArray();
            if (items.Any())
            {
                addresses.AddRange(items);
            }

			var historic = _accountService.GetHistoryAddresses();


            var hists = historic as Address[] ?? historic.ToArray();
            if (hists.Any())
            {

                foreach (var hist in hists)
                {
// ReSharper disable once AccessToForEachVariableInClosure
                    if (addresses.None(a => a.IsSame(hist)))
                    {
                        addresses.Add(hist);
                    }
                }
            }

            return addresses.Where(a => a.FullAddress.HasValue() && a.FullAddress.ToLower().StartsWith(address.ToLower())).ToArray();
        }
              

    }
}