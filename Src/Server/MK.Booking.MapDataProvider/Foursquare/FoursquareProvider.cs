using System;
using apcurium.MK.Booking.MapDataProvider;
using apcurium.MK.Booking.MapDataProvider.Resources;
using apcurium.MK.Common.Extensions;
using ServiceStack.ServiceClient.Web;
using System.Globalization;
using System.Linq;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;


namespace MK.Booking.MapDataProvider.Foursquare
{
	/// <summary>
	/// Foursquare provider.
	/// documentation : https://developer.foursquare.com/docs/venues/search
	/// </summary>
	public class FoursquareProvider : IPlaceDataProvider
	{
	    private readonly IAppSettings _settings;
	    private readonly ILogger _logger;
	    private const string ApiUrl = "https://api.foursquare.com/v2/";

        private const string SearchVenues = "venues/search?client_id={0}&client_secret={1}&intent=browse&radius={2}&v=2014100805";
        private const string VenueDetails = "venues/{0}/?client_id={1}&client_secret={2}&v=2014100805";

		public FoursquareProvider (IAppSettings settings, ILogger logger)
		{
		    _settings = settings;
		    _logger = logger;
		}

	    #region IMapsApiClient implementation

		public GeoPlace[] GetNearbyPlaces (double? latitude, double? longitude, string languageCode, bool sensor, int radius, string pipedTypeList = null)
		{
            var searchQueryString = GetBaseQueryString(latitude, longitude, radius);

		    pipedTypeList = pipedTypeList ?? _settings.Data.FoursquarePlacesTypes;
		    if (pipedTypeList.HasValue())
		    {
                searchQueryString = string.Format("{0}&categoryId={1}", searchQueryString, pipedTypeList.Replace('|', ','));
		    }

			var client = new JsonServiceClient (ApiUrl);
            var venues = client.Get<FoursquareVenuesResponse<VenuesResponse>>(searchQueryString);

			return venues.Response.Venues.Select(ToPlace).ToArray();
		}

		public GeoPlace[] SearchPlaces (double? latitude, double? longitude, string name, string languageCode, bool sensor, int radius, string countryCode)
		{
            var searchQueryString = GetBaseQueryString(latitude, longitude, radius);

		    searchQueryString = string.Format("{0}&query={1}", searchQueryString, name);

		    var client = new JsonServiceClient(ApiUrl);
            var venues = client.Get<FoursquareVenuesResponse<VenuesResponse>>(searchQueryString);

            return venues.Response.Venues.Select(ToPlace).ToArray();
		}

		private GeoPlace ToPlace(Venue venue)
	    {

			return new GeoPlace
	        {
                Name = venue.name,
                Types = venue.categories.Select(x => x.name).ToList(),
                Id = venue.id,
				Address = new GeoAddress{ FullAddress  = venue.location.address, Latitude =  venue.location.lat , Longitude = venue.location.lng },


	        };
	    }

	    private string GetBaseQueryString(double? latitude, double? longitude, int radius)
	    {
            var searchQueryString = string.Format(SearchVenues, _settings.Data.FoursquareClientId, _settings.Data.FoursquareClientSecret, radius);

	        latitude = latitude ?? _settings.Data.DefaultLatitude;
	        longitude = longitude ?? _settings.Data.DefaultLongitude;

	        searchQueryString = searchQueryString +
	                            string.Format("&ll={0},{1}", latitude.Value.ToString(CultureInfo.InvariantCulture),
	                                longitude.Value.ToString(CultureInfo.InvariantCulture));

	        return searchQueryString;
	    }

		public GeoPlace GetPlaceDetail (string id)
		{
            var client = new JsonServiceClient(ApiUrl);
			var venue = client.Get<FoursquareVenuesResponse<VenueResponse>>(string.Format(VenueDetails, id, _settings.Data.FoursquareClientId, _settings.Data.FoursquareClientSecret));
	        var location = venue.Response.Venue.location;
			string street = null;
			string streetNumber = null;
			if (!string.IsNullOrEmpty (location.address) && (char.IsNumber (location.address.FirstOrDefault ())) && location.address.Any( c=> c==' ' )) {
				streetNumber = location.address.Split (' ') [0];
				street = location.address.Substring (location.address.IndexOf (' '), location.address.Length - location.address.IndexOf (' ')).Trim(); 
			}
			return new GeoPlace{ Id = id , Name = venue.Response.Venue.name, Address = new GeoAddress
            {
                ZipCode = location.postalCode,
                Latitude = location.lat,
                Longitude = location.lng,
                State = location.state,
                City = location.city,
				FullAddress = location.address,
				Street = street,
				StreetNumber = streetNumber,

				}};
		}

	
		#endregion
	}
    
}

