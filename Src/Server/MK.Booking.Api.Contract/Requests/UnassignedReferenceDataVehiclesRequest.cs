using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/admin/vehicletypes/unassignedreference", "GET")]
    [RouteDescription("/admin/vehicletypes/unassignedreference/{VehicleBeingEdited}", "GET")]
    public class UnassignedReferenceDataVehiclesRequest
    {
        public int? VehicleBeingEdited { get; set; }
    }
}