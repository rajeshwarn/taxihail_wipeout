using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.Maps.Impl.Mappers;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Provider;
using apcurium.MK.Booking.MapDataProvider.Resources;
using apcurium.MK.Booking.MapDataProvider;

namespace apcurium.MK.Booking.Maps.Impl
{
    public class Places : IPlaces
    {
		private readonly IPlaceDataProvider _client;
        private readonly IAppSettings _appSettings;
        private readonly IPopularAddressProvider _popularAddressProvider;

        public Places(IPlaceDataProvider client, IAppSettings appSettings,
			IPopularAddressProvider popularAddressProvider)
        {
            _client = client;
            _appSettings = appSettings;
            _popularAddressProvider = popularAddressProvider;
        }

        public async Task<Address> GetPlaceDetail(string name, string placeId)
        {
            var place =  await _client.GetPlaceDetailAsync(placeId).ConfigureAwait(false);

            var result = new GeoObjToAddressMapper().ConvertToAddress(place.Address, name, true);

            return result;
        }

        public async Task<Address[]> GetFilteredPlacesList()
        {
			var filteredAddress = await _popularAddressProvider.GetPopularAddressesAsync();

			//We remove the unspecified places since those will never be used with a filter.
			return filteredAddress
                .Where(address => address.AddressLocationType != AddressLocationType.Unspecified)
                .ToArray();
        }

		public Address[] SearchPlaces(string query, double? latitude, double? longitude, string currentLanguage)
        {
            // the radius depends on what we want, if we have a query, then search larger, otherwise use nearby radius
            var radius = query.HasValueTrimmed() 
                ? _appSettings.Data.GeoLoc.SearchRadius <= 0 ? 45000 : _appSettings.Data.GeoLoc.SearchRadius
                : _appSettings.Data.NearbyPlacesService.DefaultRadius <= 0 ? 500 : _appSettings.Data.NearbyPlacesService.DefaultRadius;

            #if DEBUG
            Console.WriteLine(string.Format("Places.SearchPlaces with query: {0} radius: {1}", query.HasValueTrimmed() ? query : "(none) [nearby]", radius));
            #endif

			latitude = (!latitude.HasValue || latitude.Value == 0) ? _appSettings.Data.GeoLoc.DefaultLatitude : latitude;
			longitude = (!longitude.HasValue || longitude.Value == 0) ? _appSettings.Data.GeoLoc.DefaultLongitude : longitude;

            var popularAddresses = Enumerable.Empty<Address>();
            if (latitude.HasValue && longitude.HasValue)
            {
                if (string.IsNullOrEmpty(query))
                {
                    popularAddresses = from a in _popularAddressProvider.GetPopularAddresses()
                        select a;
                }
                else
                {
                    var words = query.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                    popularAddresses = from a in _popularAddressProvider.GetPopularAddresses()
                        where
                            words.All(
                                w =>
                                    a.FriendlyName.ToUpper().Contains(w.ToUpper()) ||
                                    a.FullAddress.ToUpper().Contains(w.ToUpper()))
                        select a;
                }

                popularAddresses = popularAddresses.ForEach(p => p.AddressType = "popular");
            }

            var places = !query.HasValueTrimmed() 
                ? _client.GetNearbyPlaces(latitude, longitude, currentLanguage, radius) 
                : _client.SearchPlaces(latitude, longitude, query, currentLanguage, radius);

            var result = popularAddresses
                .Concat(places.Select(ConvertToAddress));

            if (latitude.HasValue && longitude.HasValue)
            {
                result = result.OrderBy(p => AddressSortingHelper.GetRelevance(p, query, latitude, longitude));
            }

            // take top 15
            result = result.Take(15);

            #if DEBUG
            Console.WriteLine("Places.SearchPlaces results");
            foreach(var address in result)
            {
                Console.WriteLine(string.Format("    {0}", address.DisplayLine1));
            }
            #endif

            return result.ToArray();
        }

		private Address ConvertToAddress(GeoPlace place)
        {
            var address = new Address
            {
                Id = Guid.NewGuid(),
				PlaceId = place.Id,
                FriendlyName = place.Name,
				FullAddress = place.Address.FullAddress,
				Latitude = place.Address.Latitude,
				Longitude = place.Address.Longitude ,
                AddressType = "place",
                City = place.Address.City,
                State = place.Address.State,
                ZipCode = place.Address.ZipCode
            };

            if (address.FullAddress.HasValue() &&
                address.FullAddress.Contains("-"))
            {
                var firstWordStreetNumber = address.FullAddress.Split(' ')[0];
                if (firstWordStreetNumber.Contains("-"))
                {
                    int notImportant;
                    var isNumber = int.TryParse(firstWordStreetNumber.Split('-')[0].Trim(), out notImportant);

                    if (isNumber)
                    {
                        var newStreetNUmber = firstWordStreetNumber.Split('-')[0].Trim();
                        address.FullAddress = address.FullAddress.Replace(firstWordStreetNumber, newStreetNUmber);
                    }
                }
            }

            return address;
        }
    }
}