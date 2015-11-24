using System;
using MK.Common.Android.Serializer;
using Newtonsoft.Json;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class VehicleType
    {
        [JsonConverter(typeof(GuidJsonConverter))]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string LogoName { get; set; }

        public int ReferenceDataVehicleId { get; set; }

        public int MaxNumberPassengers { get; set; }
    }
}