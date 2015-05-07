using System;

namespace CustomerPortal.Contract.Response
{
    public class NetworkVehicleResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string LogoName { get; set; }

        public int ReferenceDataVehicleId { get; set; }

        public int MaxNumberPassengers { get; set; }
    }
}
