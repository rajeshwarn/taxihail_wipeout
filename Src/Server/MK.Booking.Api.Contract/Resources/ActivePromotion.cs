using System;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class ActivePromotion
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Code { get; set; }

        public string Progress { get; set; }

        public DateTime? ExpirationDate { get; set; }
    }
}