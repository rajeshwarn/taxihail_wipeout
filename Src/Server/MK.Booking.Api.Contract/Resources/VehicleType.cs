using apcurium.MK.Common.Enumeration;
using System;
using apcurium.MK.Common.Entity;
using Newtonsoft.Json;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class VehicleType
    {
        [JsonConverter(typeof(GuidJsonConverter))]
        public Guid Id { get; set; }

        public ServiceType ServiceType { get; set; }

        public string Name { get; set; }

        public string LogoName { get; set; }

        public int ReferenceDataVehicleId { get; set; }

        public int MaxNumberPassengers { get; set; }

        public BaseRateInfo BaseRate { get; set; }
    }
}