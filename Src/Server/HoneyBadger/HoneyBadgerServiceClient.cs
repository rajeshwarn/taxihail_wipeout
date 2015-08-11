using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Geography;
using apcurium.MK.Common.Http.Extensions;
using HoneyBadger.Enums;
using HoneyBadger.Responses;

namespace HoneyBadger
{
    public class HoneyBadgerServiceClient : BaseServiceClient
    {
        private readonly IServerSettings _serverSettings;
        private readonly ILogger _logger;

        public HoneyBadgerServiceClient(IServerSettings serverSettings, ILogger logger)
            : base(serverSettings)
        {
            _serverSettings = serverSettings;
            _logger = logger;
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
        /// <returns>The available vehicles.</returns>
        public IEnumerable<VehicleResponse> GetAvailableVehicles(string market, double latitude, double longitude, int? searchRadius = null, IList<int> fleetIds = null, bool returnAll = false)
        {
            if (fleetIds != null && !fleetIds.Any())
            {
                // No fleetId allowed for available vehicles
                return new List<VehicleResponse>();
            }

            var @params = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("includeEntities", "true"),
                    new KeyValuePair<string, string>("market", market),
                    new KeyValuePair<string, string>("meterState", ((int)MeterStates.ForHire).ToString()),
                    new KeyValuePair<string, string>("logonState", ((int)LogonStates.LoggedOn).ToString())
                };

            var searchRadiusInKm = (searchRadius ?? _serverSettings.ServerData.AvailableVehicles.Radius) / 1000;

            var vertices = GeographyHelper.CirclePointsFromRadius(latitude, longitude, searchRadiusInKm, 10);

            foreach (var vertex in vertices)
            {
                var point = string.Format("{0},{1}", vertex.Item1, vertex.Item2);
                @params.Add(new KeyValuePair<string, string>("poly", point));
            }

            if (fleetIds != null)
            {
                foreach (var fleetId in fleetIds)
                {
                    @params.Add(new KeyValuePair<string, string>("fleet", fleetId.ToString()));
                }
            }

            var honeyBadgerUrlParts = Settings.ServerData.HoneyBadger.ServiceUrl.Split('?');
            var urlParamsFromSetting = honeyBadgerUrlParts.Length > 1 ? honeyBadgerUrlParts[1] : null;

            var queryString = BuildQueryString(@params, urlParamsFromSetting);

            HoneyBadgerResponse response = null;

            try
            {
                response = Client.Get(queryString)
                                 .Deserialize<HoneyBadgerResponse>()
                                 .Result;
            }
            catch (Exception ex)
            {
                _logger.LogMessage("An error occured when trying to contact HoneyBadger");
                _logger.LogError(ex);
            }
             
            if (response != null && response.Entities != null)
            {
                var numberOfVehicles = _serverSettings.ServerData.AvailableVehicles.Count;
                var orderedVehicleList = response.Entities.OrderBy(v => v.Medallion);
                var entities = !returnAll ? orderedVehicleList.Take(numberOfVehicles) : orderedVehicleList;

                return entities.Select(e => new VehicleResponse
                                {
                                    Timestamp = e.TimeStamp,
                                    Latitude = e.Latitude,
                                    Longitude = e.Longitude,
                                    Medallion = e.Medallion,
                                    FleetId = e.FleetId,
                                    VehicleType = e.VehicleType
                                });
            }

            return new List<VehicleResponse>();
        }

        /// <summary>
        /// Method that returns the vehicles statuses for a market.
        /// </summary>
        /// <param name="market">The market to search for the vehicles statuses.</param>
        /// <param name="vehicleIds">The vehicles id (medaillon) to get the status from.
        /// If left empty, will return the status from all the vehicles in the market.</param>
        /// <param name="fleetIds">The ids of the fleets to search. (Optional)</param>
        /// <returns>The vehicle statuses.</returns>
        public IEnumerable<VehicleResponse> GetVehicleStatus(string market, IEnumerable<string> vehicleIds, IEnumerable<int> fleetIds = null)
        {
            if (vehicleIds == null)
            {
                throw new ArgumentNullException("vehicleIds");
            }

            var @params = new List<KeyValuePair<string, string>>
		    {
			    new KeyValuePair<string, string>("includeEntities", "true"),
			    new KeyValuePair<string, string>("market", market)
		    };

            foreach (var vehicleId in vehicleIds)
            {
                @params.Add(new KeyValuePair<string, string>("medallion", vehicleId));
            }

            if (fleetIds != null)
            {
                foreach (var fleetId in fleetIds)
                {
                    @params.Add(new KeyValuePair<string, string>("fleet", fleetId.ToString()));
                }
            }

            var honeyBadgerUrlParts = Settings.ServerData.HoneyBadger.ServiceUrl.Split('?');
            var urlParamsFromSetting = honeyBadgerUrlParts.Length > 1 ? honeyBadgerUrlParts[1] : null;

            var queryString = BuildQueryString(@params, urlParamsFromSetting);

            HoneyBadgerResponse response = null;

            try
            {
                response = Client.Get(queryString)
                                 .Deserialize<HoneyBadgerResponse>()
                                 .Result;
            }
            catch (Exception ex)
            {
                _logger.LogMessage("An error occured when trying to contact HoneyBadger");
                _logger.LogError(ex);
            }

            if (response != null && response.Entities != null)
            {
                return response.Entities.Select(e => new VehicleResponse
                {
                    Timestamp = e.TimeStamp,
                    Latitude = e.Latitude,
                    Longitude = e.Longitude,
                    Medallion = e.Medallion,
                    FleetId = e.FleetId
                });
            }

            return new List<VehicleResponse>();
        }
    }
}