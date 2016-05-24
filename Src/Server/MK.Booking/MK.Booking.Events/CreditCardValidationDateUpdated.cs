using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class CreditCardValidationDateUpdated : VersionedEvent
    {
        public Guid CreditCardId { get; set; }
        public DateTime? LastTokenValidateDateTime { get; set; }
        //public DateTime? LastTokenValidateDateTime { get; set; }

        //public int Year { get; set; }

        //public string DateString { get; set; }

        //public DateTime LastTokenValidateDateTimeNotNull { get; set; }

    }
}
