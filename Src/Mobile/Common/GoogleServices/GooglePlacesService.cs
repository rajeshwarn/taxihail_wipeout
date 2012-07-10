using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;

namespace TaxiMobileApp.Lib.GoogleServices
{
	public enum Ranking { Prominence = 0, Distance = 1 }

	public class GooglePlacesService : GoogleServiceClient
	{
		private const string APIKEY = "AIzaSyBzHXvi9heL8opeThi_uCIBOETLCDk575I";
		private const string URI = "maps/api/place/";

		public GooglePlacesService ( string languageCode ) : base( URI )
		{
			LanguageCode = languageCode;
			Sensor = true;
			Radius = 1000; //meters
//			RankBy = Ranking.Distance;
		}

		public bool Sensor { get; set; }
//		public Ranking RankBy { get; set; }
		public int Radius { get; set; }
		public string LanguageCode { get; set; }

		public List<GooglePlaceData> GetNearbyPlaces( double latitude, double longitude, string languageCode, bool sensor, int radius )
		{
			var request = GetRequest (Method.GET);
			request.Resource = "search/json?location={coordonnees}&language={language}&sensor={sensor}&radius={radius}&key={key}&types={types}";
			request.AddParameter ("coordonnees", string.Format("{0},{1}", latitude, longitude ), ParameterType.UrlSegment);
			request.AddParameter ("language", languageCode, ParameterType.UrlSegment);
			request.AddParameter ("sensor", sensor.ToString().ToLower(), ParameterType.UrlSegment);
			request.AddParameter ("radius", radius.ToString(), ParameterType.UrlSegment);
 			request.AddParameter ("key", APIKEY, ParameterType.UrlSegment);
			request.AddParameter ("types", new GooglePlaceTypes().GetPipedTypeList(), ParameterType.UrlSegment);

			var response = Execute<GooglePlaceResponseData<List<GooglePlaceData>>> (request);
			
			return response.results;
		}

		public List<GooglePlaceData> GetNearbyPlaces( double latitude, double longitude )
		{
			return GetNearbyPlaces( latitude, longitude, LanguageCode, Sensor, Radius );
		}

		public List<GooglePlaceData> GetNearbyPlaces( double latitude, double longitude, int radius )
		{
			return GetNearbyPlaces( latitude, longitude, LanguageCode, Sensor, radius );
		}

		public List<GooglePlaceData> GetNearbyPlaces( double latitude, double longitude, string languageCode, int radius )
		{
			return GetNearbyPlaces( latitude, longitude, languageCode, Sensor, radius );
		}
	
	
	}


}

