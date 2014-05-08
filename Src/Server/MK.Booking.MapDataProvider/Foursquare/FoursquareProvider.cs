using System;
using System.Collections.Generic;
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
	public class FoursquareProvider : IMapsApiClient
	{
	    private readonly IAppSettings _settings;
	    private readonly ILogger _logger;
	    private const string apiUrl = "https://api.foursquare.com/v2/";

        private const string searchParameters = "venues/search?client_id={0}&client_secret={1}&intent=browse&radius={2}&v=2014100805";

		private string clientId = "ZBALTMPLRQURC5BAVHJWXSGBVAPGXYQBMJ24Z1K4RHYP31YW";
		private string clientSecret = "L3AEJBI3JJILNLWTWZNVCDUIIZZ0RV3EO3RX3M44RTFKC0VM";

		public FoursquareProvider (IAppSettings settings, ILogger logger)
		{
		    _settings = settings;
		    _logger = logger;
		}

	    #region IMapsApiClient implementation

		public Place[] GetNearbyPlaces (double? latitude, double? longitude, string languageCode, bool sensor, int radius, string pipedTypeList = null)
		{
            var searchQueryString = GetBaseQueryString(latitude, longitude, radius);

		    pipedTypeList = pipedTypeList ?? _settings.Data.FoursquarePlacesTypes;
		    if (pipedTypeList.HasValue())
		    {
                searchQueryString = string.Format("{0}&categoryId={1}", searchQueryString, pipedTypeList.Replace('|', ','));
		    }

			var client = new JsonServiceClient (apiUrl);
            var venues = client.Get<FoursquareResponse>(searchQueryString);

			return venues.Response.Venues.Select (v => new Place
			{
			    Name = v.name,
                Types = v.categories.Select(x => x.name).ToList()
			}).ToArray();
		}

		public Place[] SearchPlaces (double? latitude, double? longitude, string name, string languageCode, bool sensor, int radius, string countryCode)
		{
            var searchQueryString = GetBaseQueryString(latitude, longitude, radius);

		    searchQueryString = string.Format("{0}&query={1}", searchQueryString, name);

		    var client = new JsonServiceClient(apiUrl);
            var venues = client.Get<FoursquareResponse>(searchQueryString);

            return venues.Response.Venues.Select(v => new Place
            {
                Name = v.name,
                Types = new List<string>()
            }).ToArray();
		}

	    private string GetBaseQueryString(double? latitude, double? longitude, int radius)
	    {
	        var searchQueryString = string.Format(searchParameters, clientId, clientSecret, radius);

	        latitude = latitude ?? _settings.Data.DefaultLatitude;
	        longitude = longitude ?? _settings.Data.DefaultLongitude;

	        searchQueryString = searchQueryString +
	                            string.Format("&ll={0},{1}", latitude.Value.ToString(CultureInfo.InvariantCulture),
	                                longitude.Value.ToString(CultureInfo.InvariantCulture));

	        return searchQueryString;
	    }

	    public GeoAddress GetPlaceDetail (string reference)
		{
			throw new NotImplementedException ();
		}

		/** those methods are not supported by Foursquare **/

		public DirectionResult GetDirections (double originLatitude, double originLongitude, double destinationLatitude, double destinationLongitude)
		{
			throw new NotImplementedException ();
		}

		public GeoAddress[] GeocodeAddress (string address)
		{
			throw new NotImplementedException ();
		}

		public GeoAddress[] GeocodeLocation (double latitude, double longitude)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
    
}

