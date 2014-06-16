using System;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class AccountCharge
    {
        
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Number { get; set; }

        public virtual AccountChargeQuestion[] Questions { get; set; } 
    }
}