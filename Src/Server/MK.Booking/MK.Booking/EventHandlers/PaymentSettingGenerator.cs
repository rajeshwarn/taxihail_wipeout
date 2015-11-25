#region

using System;
using System.ComponentModel.Design;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.EventHandlers
{
    public class PaymentSettingGenerator
        : IEventHandler<PaymentSettingUpdated>
    {
        private readonly Func<ConfigurationDbContext> _contextFactory;

        public PaymentSettingGenerator(Func<ConfigurationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(PaymentSettingUpdated @event)
        {
            // migration for old events, set the default value
            if (!@event.ServerPaymentSettings.PreAuthAmount.HasValue)
            {
                @event.ServerPaymentSettings.PreAuthAmount = new ServerPaymentSettings().PreAuthAmount;
            }

            using (var context = _contextFactory.Invoke())
            {
                context.RemoveWhere<ServerPaymentSettings>(x => x.Id == AppConstants.CompanyId);
                context.ServerPaymentSettings.Add(@event.ServerPaymentSettings);

                context.SaveChanges();
            }
        }
    }
}