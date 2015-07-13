using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Geography;
using CMTServices.Enums;
using CMTServices.Responses;

namespace CMTServices
{
    public abstract class BaseAvailableVehicleServiceClient
    {
        protected BaseAvailableVehicleServiceClient(IServerSettings serverSettings,ILogger logger ,string serviceUrl)
        {
            Settings = serverSettings;
            Logger = logger;

            Client = new HttpClient
            {
                BaseAddress = new Uri(serviceUrl)
            };
        }

        protected IServerSettings Settings { get; private set; }

        protected ILogger Logger { get; private set; }

        protected HttpClient Client { get; private set; }


        public abstract IEnumerable<VehicleResponse> GetAvailableVehicles(string market, double latitude, double longitude, int? searchRadius = null, IList<int> fleetIds = null, bool returnAll = false, bool wheelchairAccessibleOnly = false);

        protected static string BuildQueryString(IEnumerable<KeyValuePair<string, string>> @params, bool appendToExistingParams = false)
        {
            var requestPrefix = appendToExistingParams ? "&" : "?";

            return requestPrefix + string.Join("&", @params.Select(x => string.Join("=", x.Key, x.Value)));
        }

        protected List<KeyValuePair<string, string>> GetAvailableVehicleParams(string market, double latitude, double longitude, int? searchRadius = null, IList<int> fleetIds = null, bool returnAll = false, bool wheelchairAccessibleOnly = false, bool usePolygon = true)
        {
            if (fleetIds != null && !fleetIds.Any())
            {
                // No fleetId allowed for available vehicles
                return null;
            }

            var searchRadiusInKm = (searchRadius ?? Settings.ServerData.AvailableVehicles.Radius) / 1000;

            var @params = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("includeEntities", "true"),
                    new KeyValuePair<string, string>("market", market),
                    new KeyValuePair<string, string>("meterState", ((int)MeterStates.ForHire).ToString()),
                    new KeyValuePair<string, string>("logonState", ((int)LogonStates.LoggedOn).ToString())
                };

            if (wheelchairAccessibleOnly)
            {
                @params.Add(new KeyValuePair<string, string>("vehicleType", "1"));
            }

            if (usePolygon)
            {
                var vertices = GeographyHelper.CirclePointsFromRadius(latitude, longitude, searchRadiusInKm, 10)
                    .Select(vertex => string.Format("{0},{1}", vertex.Item1, vertex.Item2))
                    .Select(point => new KeyValuePair<string, string>("poly", point));

                @params.AddRange(vertices);
            }
            else
            {
                @params.Add(new KeyValuePair<string, string>("lat", latitude.ToString(CultureInfo.InvariantCulture)));
                @params.Add(new KeyValuePair<string, string>("lon", longitude.ToString(CultureInfo.InvariantCulture)));
                @params.Add(new KeyValuePair<string, string>("rad", (searchRadiusInKm * 1000).ToString()));
            }

            if (fleetIds != null)
            {
                @params.AddRange(fleetIds.Select(fleetId => new KeyValuePair<string, string>("fleet", fleetId.ToString())));
            }

            return @params;

        }

        protected IEnumerable<VehicleResponse> ToVehicleResponse(IEnumerable<BaseAvailableVehicleContent> entities)
        {
            return entities.Select(ToVehicleResponse);
        }

        protected VehicleResponse ToVehicleResponse(BaseAvailableVehicleContent entity)
        {
            return new VehicleResponse
            {
                Timestamp = entity.TimeStamp,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                Medallion = entity.Medallion,
                FleetId = entity.FleetId,
            };
        }
    }
}
