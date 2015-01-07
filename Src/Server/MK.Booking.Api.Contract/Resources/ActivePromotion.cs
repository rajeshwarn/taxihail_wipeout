using System;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class ActivePromotion
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Code { get; set; }

        public double? Progress { get; set; }

        public double? UnlockGoal { get; set; }

        public DateTime? ExpirationDate { get; set; }
    }
}