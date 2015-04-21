using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Http;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [NoCache]
    [Route("/admin/vehicletypes/unassignednetworkvehicletype", "GET")]
    [Route("/admin/vehicletypes/unassignednetworkvehicletype/{VehicleBeingEdited}", "GET")]
    public class UnassignedNetworkVehicleTypeRequest
    {
        public int? VehicleBeingEdited { get; set; }
    }
}
