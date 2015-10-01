using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class CreditCardLabelUpdated : VersionedEvent
    {
        public Guid CreditCardId { get; set; }
        public string Label { get; set; }
    }
}
