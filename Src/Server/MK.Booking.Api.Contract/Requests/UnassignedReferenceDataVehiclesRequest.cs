using apcurium.MK.Booking.Api.Contract.Http;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [NoCache]
    [Route("/admin/vehicletypes/unassignedreference", "GET")]
    [Route("/admin/vehicletypes/unassignedreference/{VehicleBeingEdited}", "GET")]
    public class UnassignedReferenceDataVehiclesRequest
    {
        public int? VehicleBeingEdited { get; set; }
    }
}