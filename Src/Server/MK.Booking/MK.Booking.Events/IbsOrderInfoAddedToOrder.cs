using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class IbsOrderInfoAddedToOrder_V2 : IbsOrderInfoAddedToOrder
    {
        public string CompanyKey { get; set; }
    }

    [Obsolete("Replaced by IbsOrderInfoAddedToOrder_V2", false)]
    public class IbsOrderInfoAddedToOrder : VersionedEvent
    {
        public int IBSOrderId { get; set; }

        public bool CancelWasRequested { get; set; }
    }
}