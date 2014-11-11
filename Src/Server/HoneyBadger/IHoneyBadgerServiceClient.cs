using System.Collections.Generic;
using HoneyBadger.Responses;

namespace HoneyBadger
{
    public interface IHoneyBadgerServiceClient
    {
        IEnumerable<VehicleResponse> GetAvailableVehicles(string market, string fleetId);

        string GetBestAvailableFleet(string market, IEnumerable<string> fleetIds);
    }
}
