using System;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Serializer;
using Newtonsoft.Json;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class AccountCharge
    {
        [JsonConverter(typeof(GuidJsonConverter))]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Number { get; set; }

        public bool UseCardOnFileForPayment { get; set; }

        public virtual AccountChargeQuestion[] Questions { get; set; }
    }
}