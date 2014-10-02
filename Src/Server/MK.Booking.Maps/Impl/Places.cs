using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        public Address GetPlaceDetail(string name, string referenceId)
        {
            var place = _client.GetPlaceDetail(referenceId);

			var result = new GeoObjToAddressMapper().ConvertToAddress(place.Address, name, true);

            return result;
        }

		public Address[] SearchPlaces(string name, double? latitude, double? longitude, int? radius, string currentLanguage)
        {
            int defaultRadius = _appSettings.Data.NearbyPlacesService.DefaultRadius;

            var popularAddresses = Enumerable.Empty<Address>();
            if (latitude.HasValue && longitude.HasValue)
            {
                if (string.IsNullOrEmpty(name))
                {
                    popularAddresses = from a in _popularAddressProvider.GetPopularAddresses()
                        select a;
                }
                else
                {
                    var words = name.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
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

			IEnumerable<GeoPlace> googlePlaces;

            if (string.IsNullOrWhiteSpace(name))
            {
                googlePlaces =
					_client.GetNearbyPlaces(latitude, longitude, currentLanguage, false,
                        radius.HasValue ? radius.Value : defaultRadius).Take(15);
            }
            else
            {
                var priceFormat = new RegionInfo(_appSettings.Data.PriceFormat);

                googlePlaces =
					_client.SearchPlaces(latitude, longitude, name, currentLanguage, false,
                        radius.HasValue ? radius.Value : defaultRadius, priceFormat.TwoLetterISORegionName.ToLower())
                        .Take(15);
            }

            if (latitude.HasValue && longitude.HasValue)
            {
                var places =
                    popularAddresses.Concat(googlePlaces.Select(ConvertToAddress))
                        .OrderBy(
                            p => Position.CalculateDistance(p.Latitude, p.Longitude, latitude.Value, longitude.Value))
                        .ToArray();
                return places;
            }
            else
            {
                var places = popularAddresses.Concat(googlePlaces.Select(ConvertToAddress)).ToArray();
                return places;
            }
        }

		private Address ConvertToAddress(GeoPlace place)
        {
            var address = new Address
            {
                Id = Guid.NewGuid(),
				PlaceReference = place.Id,
                FriendlyName = place.Name,
				FullAddress = place.Address.FullAddress,
				Latitude = place.Address.Latitude,
				Longitude = place.Address.Longitude ,
                AddressType = "place"
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