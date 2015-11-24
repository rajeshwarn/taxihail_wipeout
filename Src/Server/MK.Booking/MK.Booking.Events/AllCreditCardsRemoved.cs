﻿#region

using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class AllCreditCardsRemoved : VersionedEvent
    {
        public bool ForceUserDisconnect { get; set; }
    }
}