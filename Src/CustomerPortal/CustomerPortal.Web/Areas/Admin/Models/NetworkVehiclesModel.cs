using System.Collections.Generic;
using CustomerPortal.Web.Entities.Network;

namespace CustomerPortal.Web.Areas.Admin.Models
{
    public class NetworkVehiclesModel
    {
        public string Market { get; set; }

        public IEnumerable<NetworkVehicle> Vehicles { get; set; }
    }
}
