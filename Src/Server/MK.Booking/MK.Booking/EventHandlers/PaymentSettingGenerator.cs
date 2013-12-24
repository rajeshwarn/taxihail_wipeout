#region

using System;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.EventHandlers
{
    public class PaymentSettingGenerator :
        IEventHandler<PaymentSettingUpdated>
    {
        private readonly Func<ConfigurationDbContext> _contextFactory;

        public PaymentSettingGenerator(Func<ConfigurationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(PaymentSettingUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.RemoveAll<ServerPaymentSettings>();
                context.ServerPaymentSettings.Add(@event.ServerPaymentSettings);
                context.SaveChanges();
            }
        }
    }
}