using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Http.Extensions;
using CMTServices.Responses;

namespace CMTServices
{
    public class CmtGeoServiceClient : HoneyBadgerServiceClient
    {
        protected HttpClient GeoClient { get; private set; }

        public CmtGeoServiceClient(IServerSettings serverSettings, ILogger logger)
            : base(serverSettings, logger)
        {
            // create another client for the geo access
            GeoClient = new HttpClient
            {
                BaseAddress = new Uri(serverSettings.ServerData.CmtGeoSettingContainer.ServiceUrl)
            };
            GeoClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            GeoClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", serverSettings.ServerData.CmtGeoSettingContainer.AppKey);
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
                response = GeoClient.Post("/availability", ToDictionary(@params)).Deserialize<CmtGeoResponse>().Result;
            }
            catch (Exception ex)
            {
                _logger.LogMessage("An error occured when trying to contact Geo service");
                _logger.LogError(ex);
            }
             
            if (response != null && response.Entities != null)
            {
                var numberOfVehicles = _serverSettings.ServerData.AvailableVehicles.Count;
                // make sure that if ETA is null they are last in the list
                var orderedVehicleList = response.Entities.OrderBy(v => (v.ETASeconds != null ? 0 : 1)).ThenBy(v => v.ETASeconds).ThenBy(v => v.Medallion);
                var entities = !returnAll ? orderedVehicleList.Take(numberOfVehicles) : orderedVehicleList;
                return ToVehicleResponse(entities);
            }

            return new List<VehicleResponse>();
        }

        private Dictionary<string,string> ToDictionary(List<KeyValuePair<string,string>> data)
        {
            if (data == null)
            {
                return null;
            }
            var dictionary = new Dictionary<string, string>();
            data.ForEach(kv =>
            {
                if (!String.IsNullOrEmpty(kv.Value))
                {
                    dictionary.Add(kv.Key, kv.Value);
                }
            });
            return dictionary;
        }

    }
}