using apcurium.MK.Booking.Api.Contract.Http;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [NoCache]
    [Route("/admin/vehicletypes/unassignednetworkvehicletype", "GET")]
    [Route("/admin/vehicletypes/unassignednetworkvehicletype/{NetworkVehicleId}", "GET")]
    public class UnassignedNetworkVehicleTypeRequest
    {
        public int? NetworkVehicleId { get; set; }
    }
}
