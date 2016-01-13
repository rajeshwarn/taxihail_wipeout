using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using CMTServices;
using CMTServices.Responses;

namespace MK.Booking.Test.OrderStatusUpdate
{
    public class FakeHoneyBadgerServiceClient : HoneyBadgerServiceClient
    {
        public override IEnumerable<VehicleResponse> GetVehicleStatus(string market, IEnumerable<string> vehicleIds, IEnumerable<int> fleetIds = null)
        {
            return vehicleIds.Select(vehicleId => new VehicleResponse
            {
                Timestamp = DateTime.Now, Latitude = 45.3442423f, Longitude = -75.975767f, Medallion = vehicleId, FleetId = 2
            });
        }

        public FakeHoneyBadgerServiceClient(IServerSettings serverSettings) : base(serverSettings, null)
        {
        }
    }
}
