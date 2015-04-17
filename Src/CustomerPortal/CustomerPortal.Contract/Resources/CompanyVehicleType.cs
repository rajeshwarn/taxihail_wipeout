using System;

namespace CustomerPortal.Contract.Resources
{
    public class CompanyVehicleType
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string LogoName { get; set; }

        public int MaxNumberPassengers { get; set; }

        public int ReferenceDataVehicleId { get; set; }

        public int NetworkVehicleId { get; set; }
    }
}