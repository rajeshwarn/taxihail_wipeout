using System.ComponentModel.DataAnnotations;
using MongoRepository;

namespace CustomerPortal.Web.Entities.Network
{
    public class NetworkVehicle : IEntity
    {
        public string Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Market")]
        public string Market { get; set; }

        [Display(Name = "Logo")]
        public string LogoName { get; set; }

        [Display(Name = "Max # Passengers")]
        public int MaxNumberPassengers { get; set; }

        [Display(Name = "Network Vehicle Id")]
        public int NetworkVehicleId { get; set; }
    }
}