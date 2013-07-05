using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.EventHandlers
{
    public class PaymentSettingGenerator :
        IEventHandler<PaymentSettingUpdated>
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public PaymentSettingGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(PaymentSettingUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.RemoveAll<ServerPaymentSettings>();
                context.Set<ServerPaymentSettings>().Add(@event.ServerPaymentSettings);
                context.SaveChanges();
            }
        }
    }
}