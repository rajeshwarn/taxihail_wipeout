#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MonoTouch.CoreLocation;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.MapDataProvider.Resources;
using apcurium.MK.Booking.MapDataProvider;
using MonoTouch.Foundation;
using System.Text;
using MK.Booking.MapDataProvider.Foursquare;

#endregion
namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
	public class AppleMapApiClient : IMapsApiClient
	{
		private const string PlaceDetailsServiceUrl = "https://maps.googleapis.com/maps/api/place/details/";
		private const string PlacesServiceUrl = "https://maps.googleapis.com/maps/api/place/search/";
		private const string PlacesAutoCompleteServiceUrl = "https://maps.googleapis.com/maps/api/place/autocomplete/";
		private const string MapsServiceUrl = "http://maps.googleapis.com/maps/api/";
		private readonly IAppSettings _settings;
		private readonly ILogger _logger;
		private readonly string[] _otherTypesAllowed = {
			"airport", "transit_station", "bus_station", "train_station",
			"route", "postal_code", "street_address"
		};

		private IMapsApiClient _foursquare;

		public AppleMapApiClient (IAppSettings settings, ILogger logger)
		{
			_foursquare = new FoursquareProvider (settings, logger);
			_logger = logger;
			_settings = settings;
		
		}

		protected string PlacesApiKey {
			get { return _settings.Data.PlacesApiKey; }
		}

		public Place[] GetNearbyPlaces (double? latitude, double? longitude, string languageCode, bool sensor, int radius,
		                               string pipedTypeList = null)
		{
			return _foursquare.GetNearbyPlaces (latitude, longitude, languageCode, sensor, radius, pipedTypeList);
		}

		public Place[] SearchPlaces (double? latitude, double? longitude, string name, string languageCode, bool sensor,
		                            int radius, string countryCode)
		{
			return _foursquare.SearchPlaces (latitude, longitude, name, languageCode, sensor, radius, countryCode);
		}

		public GeoAddress GetPlaceDetail (string reference)
		{
			return _foursquare.GetPlaceDetail (reference);

		}

		public DirectionResult GetDirections (double originLat, double originLng, double destLat, double destLng)
		{
			var client = new JsonServiceClient (MapsServiceUrl);
			var resource = string.Format (CultureInfo.InvariantCulture,
				               "directions/json?origin={0},{1}&destination={2},{3}&sensor=true", originLat, originLng, destLat, destLng);

			return client.Get<DirectionResult> (resource);
		}

		public GeoAddress[] GeocodeAddress (string address)
		{

			var client = new JsonServiceClient (MapsServiceUrl);
			var resource = string.Format (CultureInfo.InvariantCulture, "geocode/json?address={0}&sensor=true", address);
			_logger.LogMessage ("GeocodeLocation : " + MapsServiceUrl + resource);

			var result = client.Get<GeoResult> (resource);

			if (result.Status == ResultStatus.OVER_QUERY_LIMIT || result.Status == ResultStatus.REQUEST_DENIED) {
				return GeocodeAddressUsingApple (address);
			} else {
				return ConvertGeoResultToAddresses (result);
			}

		}

		private GeoAddress[] GeocodeAddressUsingApple (string address)
		{
			var geocoder = new CLGeocoder ();

			var result = geocoder.GeocodeAddressAsync (address.Replace ("+", " "));
			result.Wait ();
			if (result.Exception != null) {
				return new GeoAddress [0];
			}

			return result.Result.Select (ConvertPlacemarkToAddress).ToArray ();			
		}

		public GeoAddress[] GeocodeLocation (double latitude, double longitude)
		{


			var geocoder = new CLGeocoder ();

			var result = geocoder.ReverseGeocodeLocationAsync (new CLLocation (latitude, longitude));
			result.Wait ();
			if (result.Exception != null) {
				return new GeoAddress [0];
			}

			return result.Result.Select (ConvertPlacemarkToAddress).ToArray ();			

		}

		private GeoAddress ConvertPlacemarkToAddress (CLPlacemark placemark)
		{		
			var streetNumber = placemark.SubThoroughfare;
			if ((streetNumber != null) && (streetNumber.Any (c => c == '–'))) {
				streetNumber = streetNumber.Substring (0, streetNumber.IndexOf ('–')); 
			}
			return  new GeoAddress { 
				StreetNumber = streetNumber,
				Street = placemark.Thoroughfare,
				Latitude = placemark.Location.Coordinate.Latitude,
				Longitude = placemark.Location.Coordinate.Longitude,
				City = placemark.Locality,
				FullAddress = placemark.Description,
				State = placemark.AdministrativeArea,
				ZipCode = placemark.PostalCode
			};
					
		}

		 

		private string GetNameFromDescription (string description)
		{
			if (string.IsNullOrWhiteSpace (description) || !description.Contains (",")) {
				return description;
			}
			var components = description.Split (',');
			return components.First ().Trim ();
		}

		private string GetAddressFromDescription (string description)
		{
			if (string.IsNullOrWhiteSpace (description) || !description.Contains (",")) {
				return description;
			}
			var components = description.Split (',');
			if (components.Count () > 1) {
				return components.Skip (1).Select (c => c.Trim ()).JoinBy (", ");
			}
			return components.First ().Trim ();
		}

		private string BuildQueryString (IDictionary<string, string> @params)
		{
			return "?" + string.Join ("&", @params.Select (x => string.Join ("=", x.Key, x.Value)));
		}

		public GeoAddress[] ConvertGeoResultToAddresses (GeoResult result)
		{ 
			if (result.Status == ResultStatus.OK) {
				return result.Results.Where (r => r.Formatted_address.HasValue () &&
				r.Geometry != null && r.Geometry.Location != null &&
				r.Geometry.Location.Lng != 0 && r.Geometry.Location.Lat != 0 &&
				(r.AddressComponentTypes.Any (
					type => type == AddressComponentType.Street_address) ||
				(r.Types.Any (
					t => _otherTypesAllowed.Any (o => o.ToLower () == t.ToLower ())))))
						.Select (ConvertGeoObjectToAddress).ToArray ();
			} else {
				return new GeoAddress[0];
			}


		}

		public GeoAddress ConvertGeoObjectToAddress (GeoObj geoResult)
		{

			var address = new GeoAddress {

				FullAddress = geoResult.Formatted_address,                
				Latitude = geoResult.Geometry.Location.Lat,
				Longitude = geoResult.Geometry.Location.Lng
			};

			geoResult.Address_components.FirstOrDefault (
				x => x.AddressComponentTypes.Any (t => t == AddressComponentType.Street_number))
				.Maybe (x => address.StreetNumber = x.Long_name);

			var component = (from c in geoResult.Address_components
			                 where
			                     (c.AddressComponentTypes.Any (
				                     x => x == AddressComponentType.Route || x == AddressComponentType.Street_address) &&
			                     !string.IsNullOrEmpty (c.Long_name))
			                 select c).FirstOrDefault ();
			component.Maybe (c => address.Street = c.Long_name);

			geoResult.Address_components.FirstOrDefault (
				x => x.AddressComponentTypes.Any (t => t == AddressComponentType.Postal_code))
				.Maybe (x => address.ZipCode = x.Long_name);

			geoResult.Address_components.FirstOrDefault (
				x => x.AddressComponentTypes.Any (t => t == AddressComponentType.Locality))
				.Maybe (x => address.City = x.Long_name);

			geoResult.Address_components.FirstOrDefault (
				x => x.AddressComponentTypes.Any (t => t == AddressComponentType.Administrative_area_level_1))
				.Maybe (x => address.State = x.Short_name);

			address.LocationType = geoResult.Geometry.Location_type;

			return address;
		}
	}
}

