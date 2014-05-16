#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.MapDataProvider.Resources;
using apcurium.MK.Booking.MapDataProvider.Google.Resources;

#endregion
namespace apcurium.MK.Booking.MapDataProvider.Google
{
	public class GoogleApiClient : IPlaceDataProvider, IDirectionDataProvider, IGeocoder
	{
		private const string PlaceDetailsServiceUrl = "https://maps.googleapis.com/maps/api/place/details/";
		private const string PlacesServiceUrl = "https://maps.googleapis.com/maps/api/place/search/";
		private const string PlacesAutoCompleteServiceUrl = "https://maps.googleapis.com/maps/api/place/autocomplete/";
		private const string MapsServiceUrl = "http://maps.googleapis.com/maps/api/";
		private readonly IAppSettings _settings;
		private readonly ILogger _logger;
		private readonly IGeocoder _fallbackGeocoder;
		private readonly string[] _otherTypesAllowed = {
            "airport", "transit_station", "bus_station", "train_station",
            "route", "postal_code", "street_address"
        };
		public GoogleApiClient(IAppSettings settings, ILogger logger, IGeocoder fallbackGeocoder = null)
        {
            _logger = logger;
            _settings = settings;
			_fallbackGeocoder = fallbackGeocoder;
        }

        protected string PlacesApiKey
        {
            get { return _settings.Data.PlacesApiKey; }
        }

		public GeoPlace[] GetNearbyPlaces(double? latitude, double? longitude, string languageCode, bool sensor, int radius,
            string pipedTypeList = null)
        {
            pipedTypeList = pipedTypeList ?? new PlaceTypes(_settings.Data.PlacesTypes).GetPipedTypeList();
            var client = new JsonServiceClient(PlacesServiceUrl);

            var @params = new Dictionary<string, string>
            {
                {"sensor", sensor.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()},
                {"key", PlacesApiKey},
                {"radius", radius.ToString(CultureInfo.InvariantCulture)},
                {"language", languageCode},
                {"types", pipedTypeList},
            };


            if (latitude != null
                && longitude != null)
            {
                @params.Add("location",
                    string.Join(",", latitude.Value.ToString(CultureInfo.InvariantCulture),
                        longitude.Value.ToString(CultureInfo.InvariantCulture)));
            }


            var r = "json" + BuildQueryString(@params);

            _logger.LogMessage("Nearby Places API : " + PlacesServiceUrl + r);

			return client.Get<PlacesResponse>(r).Results.Select( ConvertPlaceToGeoPlaces ).ToArray();
        }

		public GeoPlace[] SearchPlaces(double? latitude, double? longitude, string name, string languageCode, bool sensor,
            int radius, string countryCode)
        {
            var url = PlacesAutoCompleteServiceUrl;
            var client = new JsonServiceClient(url);

            var @params = new Dictionary<string, string>
            {
                {"sensor", sensor.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()},
                {"key", PlacesApiKey},
                {"radius", radius.ToString(CultureInfo.InvariantCulture)},
                {"language", languageCode},
                {"types", "establishment"},
                {"components", "country:" + countryCode},
            };


            if (latitude != null
                && longitude != null)
            {
                @params.Add("location",
                    string.Join(",", latitude.Value.ToString(CultureInfo.InvariantCulture),
                        longitude.Value.ToString(CultureInfo.InvariantCulture)));
            }

            if (name != null)
            {
                @params.Add("input", name);
            }

            var r = "json" + BuildQueryString(@params);


            _logger.LogMessage("Search Places API : " + url + r);


            var result = client.Get<PredictionResponse>(r).predictions;

            return ConvertPredictionToPlaces(result).ToArray();
        }

		public GeoPlace GetPlaceDetail(string id)
        {
            var client = new JsonServiceClient(PlaceDetailsServiceUrl);
            var @params = new Dictionary<string, string>
            {
				{"reference", id},
                {"sensor", true.ToString().ToLower()},
                {"key", PlacesApiKey},
            };

            var qry = "json" + BuildQueryString(@params);
            Console.WriteLine(qry);

			var placeResponse = client.Get<PlaceDetailResponse> (qry).Result;
			return new GeoPlace {
				Id = id,
				Name = placeResponse.Formatted_address,
				Address = ConvertGeoObjectToAddress (placeResponse)
			};
            
        }

		public GeoDirection GetDirections(double originLat, double originLng, double destLat, double destLng)
        {
            var client = new JsonServiceClient(MapsServiceUrl);
            var resource = string.Format(CultureInfo.InvariantCulture,
                "directions/json?origin={0},{1}&destination={2},{3}&sensor=true", originLat, originLng, destLat, destLng);

			var direction = client.Get<DirectionResult>(resource);
			if (direction.Status == ResultStatus.OK)
			{
				var route = direction.Routes.ElementAt(0);
				if (route.Legs.Count > 0)
				{
					var distance = route.Legs.Sum(leg => leg.Distance.Value);
					var duration = route.Legs.Sum(leg => leg.Duration.Value);
					return new GeoDirection{ Distance = distance, Duration = duration };
				}
			}

			return new GeoDirection ();
			//return 
        }

        public GeoAddress[] GeocodeAddress(string address)
        {
            var client = new JsonServiceClient(MapsServiceUrl);
            var resource = string.Format(CultureInfo.InvariantCulture, "geocode/json?address={0}&sensor=true", address);
            _logger.LogMessage("GeocodeLocation : " + MapsServiceUrl + resource);

            var result = client.Get<GeoResult>(resource);

			if ( (result.Status == ResultStatus.OVER_QUERY_LIMIT || result.Status == ResultStatus.REQUEST_DENIED) && (_fallbackGeocoder != null )) {
				return _fallbackGeocoder.GeocodeAddress (address);
			} else if (result.Status == ResultStatus.OK) {
				return ConvertGeoResultToAddresses(result);
			} else {
				return new GeoAddress [0];
			}

            
           
        }

        public GeoAddress[] GeocodeLocation(double latitude, double longitude)
        {
            var client = new JsonServiceClient(MapsServiceUrl);

            var resource = string.Format(CultureInfo.InvariantCulture, "geocode/json?latlng={0},{1}&sensor=true",
                latitude, longitude);

            _logger.LogMessage("GeocodeLocation : " + MapsServiceUrl + resource);

            
            var result = client.Get<GeoResult>(resource);
			if ( (result.Status == ResultStatus.OVER_QUERY_LIMIT || result.Status == ResultStatus.REQUEST_DENIED) && (_fallbackGeocoder != null )) {
				return _fallbackGeocoder.GeocodeLocation (latitude, longitude);
			} else if (result.Status == ResultStatus.OK) {
				return ConvertGeoResultToAddresses (result);
			} else {
				return new GeoAddress [0];
			}
           
        }


				
		private GeoPlace ConvertPlaceToGeoPlaces(Place place)
		{            
			return
							new GeoPlace {
				Id = place.Reference,
				Name =  place.Name,
				Address = new GeoAddress { FullAddress = place.Formatted_Address ?? place.Vicinity, Latitude = place.Geometry.Location.Lat ,Longitude = place.Geometry.Location.Lng  },                                                       
				Types = place.Types

			};
		}


		private IEnumerable<GeoPlace> ConvertPredictionToPlaces(IEnumerable<Prediction> result)
        {            
            return
                result.Select(
                    p =>
					new GeoPlace
                        {
								Id = p.Reference,
                            	Name = GetNameFromDescription(p.Description),
								Address = new GeoAddress { FullAddress =  GetAddressFromDescription(p.Description)  },                                                       
                            	Types = p.Types
                        });
        }

        private string GetNameFromDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description) || !description.Contains(","))
            {
                return description;
            }
            var components = description.Split(',');
            return components.First().Trim();
        }

        private string GetAddressFromDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description) || !description.Contains(","))
            {
                return description;
            }
            var components = description.Split(',');
            if (components.Count() > 1)
            {
                return components.Skip(1).Select(c => c.Trim()).JoinBy(", ");
            }
            return components.First().Trim();
        }

        private string BuildQueryString(IDictionary<string, string> @params)
        {
            return "?" + string.Join("&", @params.Select(x => string.Join("=", x.Key, x.Value)));
        }

        public GeoAddress[] ConvertGeoResultToAddresses(GeoResult result)
        { 
            if ( result.Status == ResultStatus.OK )
            {
                return result.Results.Where(r=> r.Formatted_address.HasValue() &&
                                                r.Geometry != null && r.Geometry.Location != null &&
                                                r.Geometry.Location.Lng != 0 && r.Geometry.Location.Lat != 0 &&
                                                (r.AddressComponentTypes.Any(
                                                    type => type == AddressComponentType.Street_address) ||
                                                 (r.Types.Any(
                                                     t => _otherTypesAllowed.Any(o => o.ToLower() == t.ToLower())))))
                                                     .Select(ConvertGeoObjectToAddress).ToArray();
            }
            else
            {
                return new GeoAddress[0];
            }

         
        }

        public GeoAddress ConvertGeoObjectToAddress(GeoObj geoResult)
        {
                        
            var address = new GeoAddress
            {
            
                FullAddress = geoResult.Formatted_address,                
                Latitude = geoResult.Geometry.Location.Lat,
                Longitude = geoResult.Geometry.Location.Lng
            };

            geoResult.Address_components.FirstOrDefault(
                x => x.AddressComponentTypes.Any(t => t == AddressComponentType.Street_number))
                .Maybe(x => address.StreetNumber = x.Long_name);

            var component = (from c in geoResult.Address_components
                             where
                                 (c.AddressComponentTypes.Any(
                                     x => x == AddressComponentType.Route || x == AddressComponentType.Street_address) &&
                                  !string.IsNullOrEmpty(c.Long_name))
                             select c).FirstOrDefault();
            component.Maybe(c => address.Street = c.Long_name);

            geoResult.Address_components.FirstOrDefault(
                x => x.AddressComponentTypes.Any(t => t == AddressComponentType.Postal_code))
                .Maybe(x => address.ZipCode = x.Long_name);

            geoResult.Address_components.FirstOrDefault(
                x => x.AddressComponentTypes.Any(t => t == AddressComponentType.Locality))
                .Maybe(x => address.City = x.Long_name);

            geoResult.Address_components.FirstOrDefault(
                x => x.AddressComponentTypes.Any(t => t == AddressComponentType.Administrative_area_level_1))
                .Maybe(x => address.State = x.Short_name);
            
            address.LocationType = geoResult.Geometry.Location_type;

			return address;
        }

    }
}