﻿#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using apcurium.MK.Booking.Google.Resources;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using ServiceStack.ServiceClient.Web;
using ServiceStack.Text;

#endregion

namespace apcurium.MK.Booking.Google.Impl
{
    public class MapsApiClient : IMapsApiClient
    {
        private const string PlaceDetailsServiceUrl = "https://maps.googleapis.com/maps/api/place/details/";
        private const string PlacesServiceUrl = "https://maps.googleapis.com/maps/api/place/search/";
        private const string PlacesAutoCompleteServiceUrl = "https://maps.googleapis.com/maps/api/place/autocomplete/";
        private const string MapsServiceUrl = "http://maps.googleapis.com/maps/api/";

        private readonly IAppSettings _settings;
        private readonly ILogger _logger;

        public MapsApiClient(IAppSettings settings, ILogger logger)
        {
            _logger = logger;
            _settings = settings;
        }

        protected string PlacesApiKey
        {
            get { return _settings.Data.PlacesApiKey; }
        }

        public Place[] GetNearbyPlaces(double? latitude, double? longitude, string languageCode, bool sensor, int radius,
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

            return client.Get<PlacesResponse>(r).Results.ToArray();
        }

        public Place[] SearchPlaces(double? latitude, double? longitude, string name, string languageCode, bool sensor,
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

        public GeoObj GetPlaceDetail(string reference)
        {
            var client = new JsonServiceClient(PlaceDetailsServiceUrl);
            var @params = new Dictionary<string, string>
            {
                {"reference", reference},
                {"sensor", true.ToString().ToLower()},
                {"key", PlacesApiKey},
            };

            var qry = "json" + BuildQueryString(@params);
            Console.WriteLine(qry);
            return client.Get<PlaceDetailResponse>(qry).Result;
        }

        public DirectionResult GetDirections(double originLat, double originLng, double destLat, double destLng)
        {
            var client = new JsonServiceClient(MapsServiceUrl);
            var resource = string.Format(CultureInfo.InvariantCulture,
                "directions/json?origin={0},{1}&destination={2},{3}&sensor=true", originLat, originLng, destLat, destLng);

            return client.Get<DirectionResult>(resource);
        }

        public GeoResult GeocodeAddress(string address)
        {
            var client = new JsonServiceClient(MapsServiceUrl);
            var resource = string.Format(CultureInfo.InvariantCulture, "geocode/json?address={0}&sensor=true", address);

			_logger.LogMessage("GeocodeAddress : " + MapsServiceUrl + resource);

			var result = client.Get<GeoResult>(resource);

			_logger.LogMessage ("GeocodeAddress result : " + result.ToJson ());

			return result;
        }

        public GeoResult GeocodeLocation(double latitude, double longitude)
        {
            var client = new JsonServiceClient(MapsServiceUrl);

            var resource = string.Format(CultureInfo.InvariantCulture, "geocode/json?latlng={0},{1}&sensor=true",
                latitude, longitude);

            _logger.LogMessage("GeocodeLocation : " + MapsServiceUrl + resource);

			var result =  client.Get<GeoResult>(resource);

			_logger.LogMessage ("GeocodeLocation result : " + result.ToJson ());

			return result;
        }

        private IEnumerable<Place> ConvertPredictionToPlaces(IEnumerable<Prediction> result)
        {
            var g = new Geometry {Location = new Location {Lat = 0, Lng = 0}};
            return
                result.Select(
                    p =>
                        new Place
                        {
                            Id = p.Id,
                            Name = GetNameFromDescription(p.Description),
                            Reference = p.Reference,
                            Formatted_Address = GetAddressFromDescription(p.Description),
                            Geometry = g,
                            Vicinity = "n\a",
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
    }
}