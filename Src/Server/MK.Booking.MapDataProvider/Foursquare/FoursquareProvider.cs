using System;
using apcurium.MK.Booking.MapDataProvider;
using apcurium.MK.Booking.MapDataProvider.Resources;
using apcurium.MK.Common.Extensions;
using ServiceStack.ServiceClient.Web;
using System.Globalization;
using System.Linq;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using System.Collections.Generic;

namespace MK.Booking.MapDataProvider.Foursquare
{
	/// <summary>
	/// Foursquare provider.
	/// api reference:
	/// https://developer.foursquare.com/docs/venues/search
	/// https://developer.foursquare.com/docs/venues/explore
	/// </summary>
	public class FoursquareProvider : IPlaceDataProvider
	{
		enum FoursquareQueryType
		{
			Search,
			Explore
		}


	    private readonly IAppSettings _settings;
	    private readonly ILogger _logger;

		static readonly uint maximumPageLength = 50, maximumPagesLimit = 5;

		static readonly string ApiUrl = "https://api.foursquare.com/v2/";
		static readonly string SearchVenues = "venues/search?v=20140806&m=foursquare&client_id={0}&client_secret={1}&intent=browse&radius={2}&limit=" + maximumPageLength.ToString();
		static readonly string ExploreVenues = "venues/explore?v=20140806&m=foursquare&client_id={0}&client_secret={1}&radius={2}&limit=" + maximumPageLength.ToString();
		static readonly string VenueDetails = "venues/{0}/?v=20140806&m=foursquare&client_id={1}&client_secret={2}";


        public FoursquareProvider(IAppSettings settings, ILogger logger)
		{
		    _settings = settings;
		    _logger = logger;
		}

		public GeoPlace[] GetNearbyPlaces (double? latitude, double? longitude, string languageCode, bool sensor, int radius, string pipedTypeList = null)
		{
			var searchQueryString = GetBaseQueryString(latitude, longitude, radius, FoursquareQueryType.Search);

			pipedTypeList = pipedTypeList ?? _settings.Data.FoursquarePlacesTypes;
			if (pipedTypeList.HasValue())
			{
				searchQueryString = string.Format("{0}&categoryId={1}", searchQueryString, pipedTypeList.Replace('|', ','));
			}

			var client = new JsonServiceClient(ApiUrl);
			var searchAnswer = client.Get<FoursquareVenuesResponse<SearchResponse>>(searchQueryString);

			List<Venue> venuesSearch = new List<Venue>();
			if (searchAnswer.Response.Venues != null && searchAnswer.Response.Venues.Length > 0)
			{
				venuesSearch.AddRange(searchAnswer.Response.Venues);
			}



			uint page = 0, pages = 0;

			var exploreQuery = GetBaseQueryString(latitude, longitude, radius, FoursquareQueryType.Explore);
			FoursquareVenuesResponse<ExploreResponse> exploreAnswer;

			List<Venue> venuesExplore = new List<Venue>();

			do
			{
				exploreAnswer = client.Get<FoursquareVenuesResponse<ExploreResponse>>(exploreQuery + "&offset=" + ((page++) * maximumPageLength).ToString());

				pages = Math.Min((uint)(exploreAnswer.Response.TotalResults / maximumPageLength) + 1, maximumPagesLimit);

				venuesExplore.AddRange(
						from q1 in exploreAnswer.Response.Groups
						from q2 in q1.Items
						select q2.Venue);

			}
			while (page < pages && exploreAnswer.Response.Groups != null && exploreAnswer.Response.Groups.Length > 0);


			List<Venue> allVenues = new List<Venue>();

			allVenues.AddRange(venuesExplore);
			allVenues.AddRange(from vs in venuesSearch
							   where !(from ve in venuesExplore select ve.id).Contains(vs.id)
							   select vs);

			return allVenues.Select(ToPlace).ToArray();
		}

		public GeoPlace[] SearchPlaces (double? latitude, double? longitude, string name, string languageCode, bool sensor, int radius, string countryCode)
		{
            var searchQueryString = GetBaseQueryString(latitude, longitude, radius, FoursquareQueryType.Search);

		    searchQueryString = string.Format("{0}&query={1}", searchQueryString, name);

		    var client = new JsonServiceClient(ApiUrl);
            var venues = client.Get<FoursquareVenuesResponse<SearchResponse>>(searchQueryString);

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

		private string GetBaseQueryString(double? latitude, double? longitude, int radius, FoursquareQueryType foursquareQueryType)
	    {
			string template = null;

			if (foursquareQueryType == FoursquareQueryType.Search)
			{
				template = SearchVenues;
			}
			else if (foursquareQueryType == FoursquareQueryType.Explore)
			{
				template = ExploreVenues;
			}

            var searchQueryString = string.Format(template, _settings.Data.FoursquareClientId, _settings.Data.FoursquareClientSecret, radius);

            latitude = latitude ?? _settings.Data.GeoLoc.DefaultLatitude;
            longitude = longitude ?? _settings.Data.GeoLoc.DefaultLongitude;

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
	}
}