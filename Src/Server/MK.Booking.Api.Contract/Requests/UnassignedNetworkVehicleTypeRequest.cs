using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/admin/vehicletypes/unassignednetworkvehicletype", "GET")]
    [RouteDescription("/admin/vehicletypes/unassignednetworkvehicletype/{NetworkVehicleId}", "GET")]
    public class UnassignedNetworkVehicleTypeRequest
    {
        public int? NetworkVehicleId { get; set; }
    }
}
