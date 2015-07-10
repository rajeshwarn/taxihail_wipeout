﻿using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Geography;
using apcurium.MK.Common.Http.Extensions;
using CMTServices.Enums;
using CMTServices.Responses;

namespace CMTServices
{
    public class HoneyBadgerServiceClient : BaseAvailableVehicleServiceClient
    {
        public HoneyBadgerServiceClient(IServerSettings serverSettings, ILogger logger)
            : base(serverSettings, logger, serverSettings.ServerData.HoneyBadger.ServiceUrl)
        {
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
            var @params = GetAvailableVehicleParams(market, latitude, longitude, searchRadius, fleetIds, returnAll, wheelchairAccessibleOnly);
            if (@params == null)
            {
                return new List<VehicleResponse>();
            }
            var appendToExistingParams = Settings.ServerData.HoneyBadger.ServiceUrl.Contains("?");

            var queryString = BuildQueryString(@params, appendToExistingParams);

            HoneyBadgerResponse response = null;

            try
            {
                response = Client.Get(queryString)
                                 .Deserialize<HoneyBadgerResponse>()
                                 .Result;
            }
            catch (Exception ex)
            {
                Logger.LogMessage("An error occured when trying to contact HoneyBadger");
                Logger.LogError(ex);
            }
             
            if (response != null && response.Entities != null)
            {
                var numberOfVehicles = Settings.ServerData.AvailableVehicles.Count;
                var orderedVehicleList = response.Entities.OrderBy(v => v.Medallion);
                var entities = !returnAll ? orderedVehicleList.Take(numberOfVehicles) : orderedVehicleList;
                return ToVehicleResponse(entities);
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

            @params.AddRange(vehicleIds.Select(vehicleId => new KeyValuePair<string, string>("medallion", vehicleId)));

            if (fleetIds != null)
            {
                @params.AddRange(fleetIds.Select(fleetId => new KeyValuePair<string, string>("fleet", fleetId.ToString())));
            }

            var appendToExistingParams = Settings.ServerData.HoneyBadger.ServiceUrl.Contains("?");

            var queryString = BuildQueryString(@params, appendToExistingParams);

            HoneyBadgerResponse response = null;

            try
            {
                response = Client.Get(queryString)
                                 .Deserialize<HoneyBadgerResponse>()
                                 .Result;
            }
            catch (Exception ex)
            {
                Logger.LogMessage("An error occured when trying to contact HoneyBadger");
                Logger.LogError(ex);
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