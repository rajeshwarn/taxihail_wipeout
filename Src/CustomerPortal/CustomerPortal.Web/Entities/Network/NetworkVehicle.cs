using MongoRepository;
using System.ComponentModel.DataAnnotations;

namespace CustomerPortal.Web.Entities.Network
{
    public class NetworkVehicle : IEntity
    {
        public string Id { get; set; }

        public int NetworkVehicleId { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Logo")]
        public string LogoName { get; set; }

        [Display(Name = "Max # Passengers")]
        public int MaxNumberPassengers { get; set; }

        public string Market { get; set; }
    }
}