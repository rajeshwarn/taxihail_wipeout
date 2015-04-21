using apcurium.MK.Booking.Api.Contract.Http;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [NoCache]
    [Route("/admin/vehicletypes/unassignednetworkvehicletype", "GET")]
    [Route("/admin/vehicletypes/unassignednetworkvehicletype/{VehicleIdBeingEdited}", "GET")]
    public class UnassignedNetworkVehicleTypeRequest
    {
        public int? VehicleIdBeingEdited { get; set; }
    }
}
