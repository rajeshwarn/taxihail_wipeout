using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Http.Extensions;
using CMTServices.Responses;
using ServiceStack.Common;

namespace CMTServices
{
    public class CmtGeoServiceClient : BaseAvailableVehicleServiceClient
    {
        public CmtGeoServiceClient(IServerSettings serverSettings, ILogger logger)
            : base(serverSettings, logger, serverSettings.ServerData.CmtGeo.ServiceUrl)
        {
            // create another client for the geo access
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", serverSettings.ServerData.CmtGeo.AppKey);
        }

        /// <summary>
        /// Method that returns the vehicles available for a market.
        /// </summary>
        /// <param name="market">The market to search for available vehicles.</param>
        /// <param name="latitude">Search origin latitude.</param>
        /// <param name="longitude">Search origin longitude</param>
        /// <param name="searchRadius">Search radius in meters (Optional)</param>
        /// <param name="fleetIds">The ids of the fleets to search. (Optional)</param>
        /// <param name="returnAll">True to return all the available vehicles; false will return a set number defined by the admin settings. (Optional)</param>
        /// <param name="wheelchairAccessibleOnly">True to return only wheelchair accessible vehicles, false will return all. (Optional)</param>
        /// <returns>The available vehicles.</returns>
        public override IEnumerable<VehicleResponse> GetAvailableVehicles(string market, double latitude, double longitude, int? searchRadius = null, IList<int> fleetIds = null, bool returnAll = false, bool wheelchairAccessibleOnly = false)
        {
            var @params = GetAvailableVehicleParams(market, latitude, longitude, searchRadius, fleetIds, returnAll, wheelchairAccessibleOnly, false);
            if (@params == null)
            {
                return new List<VehicleResponse>();
            }

            // the following have different defaults from badger service
            @params.Add(new KeyValuePair<string,string>("includeMapMatch", "true"));
            @params.Add(new KeyValuePair<string,string>("includeETA", "true"));
            
            // required for geo service
            @params.Add(new KeyValuePair<string, string>("limit", "12"));

            CmtGeoResponse response = null;
            try
            {
                response = Client.Post("/availability", ToDictionary(@params))
                    .Deserialize<CmtGeoResponse>()
                    .Result;
            }
            catch (Exception ex)
            {
                Logger.LogMessage("An error occured when trying to contact Geo service");
                Logger.LogError(ex);
            }
             
            if (response != null && response.Entities != null)
            {
                var numberOfVehicles = Settings.ServerData.AvailableVehicles.Count;
                // make sure that if ETA is null they are last in the list
                var orderedVehicleList = response.Entities
                    .OrderBy(v => (v.ETASeconds != null ? 0 : 1))
                    .ThenBy(v => v.ETASeconds)
                    .ThenBy(v => v.Medallion);

                var entities = !returnAll 
                    ? orderedVehicleList.Take(numberOfVehicles) 
                    : orderedVehicleList;

                return ToVehicleResponse(entities);
            }

            return new List<VehicleResponse>();
        }

        protected IEnumerable<VehicleResponse> ToVehicleResponse(IEnumerable<CmtGeoContent> entities)
        {
            return entities.Select(ToVehicleResponse);
        }

        protected VehicleResponse ToVehicleResponse(CmtGeoContent entity)
        {
            var response = base.ToVehicleResponse(entity);

            response.Eta = entity.ETASeconds;

            return response;
        }

        public VehicleResponse GetEta(double latitude, double longitude, string vehicleRegistration)
        {
            var @params = new []
            {
                new KeyValuePair<string, object>("lat", latitude),
                new KeyValuePair<string, object>("lon", longitude),
                new KeyValuePair<string, object>("deviceName", vehicleRegistration)
            };

            try
            {
                var response = Client.Post("/eta", @params.ToDictionary(kv => kv.Key, kv => kv.Value))
                    .Deserialize<CmtGeoContent>()
                    .Result;

                return ToVehicleResponse(response);
            }
            catch (Exception ex)
            {
                Logger.LogMessage("An error occured when trying to contact CMT Geo service");
                Logger.LogError(ex);

                return new VehicleResponse();
            }
        }

        private Dictionary<string,string> ToDictionary(IEnumerable<KeyValuePair<string,string>> data)
        {
            if (data == null)
            {
                return null;
            }

            return data
                .Where(kv => kv.Value.HasValue())
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

    }
}