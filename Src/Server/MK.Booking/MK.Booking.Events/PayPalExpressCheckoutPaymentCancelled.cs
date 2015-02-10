#region

using System;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    [Obsolete("This event is deprecated. PayPal express checkout is not longer supported.")]
    public class PayPalExpressCheckoutPaymentCancelled : VersionedEvent
    {
    }
}