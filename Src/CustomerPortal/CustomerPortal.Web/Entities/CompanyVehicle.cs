using MongoRepository;

namespace CustomerPortal.Web.Entities
{
    public class CompanyVehicle : IEntity
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string LogoName { get; set; }

        public int MaxNumberPassengers { get; set; }

        public int ReferenceDataVehicleId { get; set; }

        public int? NetworkVehicleId { get; set; }
    }
}
