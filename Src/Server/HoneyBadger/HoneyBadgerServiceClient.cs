using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Geography;
using HoneyBadger.Enums;
using HoneyBadger.Extensions;
using HoneyBadger.Responses;

namespace HoneyBadger
{
    public class HoneyBadgerServiceClient : BaseServiceClient
    {
        private readonly IServerSettings _serverSettings;

        public HoneyBadgerServiceClient(IServerSettings serverSettings)
        {
            _serverSettings = serverSettings;
        }

        /// <summary>
        /// Method that returns the vehicles available for a market.
        /// </summary>
        /// <param name="market">The market to search for available vehicles.</param>
        /// <param name="latitude">Search origin latitude.</param>
        /// <param name="longitude">Search origin longitude</param>
        /// <param name="searchRadius">Search radius in meters</param>
        /// <param name="fleetId">The id of the fleet to search.</param>
        /// <param name="returnAll">True to return all the available vehicles; false will return a set number defined by the admin settings.</param>
        /// <returns>The available vehicles.</returns>
        public IEnumerable<VehicleResponse> GetAvailableVehicles(string market, double latitude, double longitude, int? searchRadius = null, string fleetId = null, bool returnAll = false)
        {
            var searchRadiusInKm = (searchRadius ?? _serverSettings.ServerData.AvailableVehicles.Radius) / 1000;
            var numberOfVehicles = _serverSettings.ServerData.AvailableVehicles.Count;

            var @params = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("includeEntities", "true"),
                    new KeyValuePair<string, string>("market", market),
                    new KeyValuePair<string, string>("meterState", ((int)MeterStates.ForHire).ToString()),
                };

            var vertices = GeographyhHelper.CirclePointsFromRadius(latitude, longitude, searchRadiusInKm, 10);

            foreach (var vertex in vertices)
            {
                var point = string.Format("{0},{1}", vertex.Item1, vertex.Item2);
                @params.Add(new KeyValuePair<string, string>("poly", point));
            }

            if (fleetId.HasValue())
            {
                @params.Add(new KeyValuePair<string, string>("fleet", fleetId));
            }

            var queryString = BuildQueryString(@params);

            var response = Client.Get("availability" + queryString)
                                 .Deserialize<HoneyBadgerResponse>()
                                 .Result;

            if (response.Entities != null)
            {
                var entities = !returnAll ? response.Entities.Take(numberOfVehicles) : response.Entities;
                return entities.Select(e => new VehicleResponse
                                {
                                    Timestamp = e.TimeStamp,
                                    Latitude = e.Latitude,
                                    Longitude = e.Longitude,
                                    Medaillon = e.Medaillon,
                                    FleetId = e.FleetId
                                });
            }

            return new List<VehicleResponse>();
        }
    }
}