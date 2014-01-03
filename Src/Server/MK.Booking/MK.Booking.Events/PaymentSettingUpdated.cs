#region

using apcurium.MK.Common.Configuration.Impl;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class PaymentSettingUpdated : VersionedEvent
    {
        public ServerPaymentSettings ServerPaymentSettings { get; set; }
    }
}