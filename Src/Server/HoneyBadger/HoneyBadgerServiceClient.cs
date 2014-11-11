using System;
using System.Collections.Generic;
using System.Linq;
using HoneyBadger.Enums;
using HoneyBadger.Extensions;
using HoneyBadger.Responses;

namespace HoneyBadger
{
    public class HoneyBadgerServiceClient : BaseServiceClient, IHoneyBadgerServiceClient
    {
        public IEnumerable<VehicleResponse> GetAvailableVehicles(string market, string fleetId)
        {
            var @params = new Dictionary<string, string>
                {
                    { "market", market },
                    { "fleetId", fleetId },
                    { "includeEntities", "true" }
                };

            var queryString = BuildQueryString(@params);

            var response = Client.Get("availability" + queryString)
                                 .Deserialize<HoneyBadgerResponse>()
                                 .Result;

            // Select all vehicles that are available
            var availableVehicles = response.Entities.Where(e => e.LogonState == LogonStates.LoggedOn
                && e.MeterState == MeterStates.ForHire);

            return availableVehicles.Select(e => new VehicleResponse
            {
                Timestamp = e.TimeStamp,
                Latitude = e.Latitude,
                Longitude = e.Longitude
            });
        }

        public string GetBestAvailableFleet(string market, IEnumerable<string> fleetIds)
        {
            // Waiting on MK for this one. Will be completed part of MKTAXI-2283.
            throw new NotImplementedException();
        }
    }
}
