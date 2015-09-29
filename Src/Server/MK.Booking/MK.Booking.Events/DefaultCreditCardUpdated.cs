using Infrastructure.EventSourcing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Events
{
    public class DefaultCreditCardUpdated : VersionedEvent
    {
        public Guid CreditCardId { get; set; }
    }
}
