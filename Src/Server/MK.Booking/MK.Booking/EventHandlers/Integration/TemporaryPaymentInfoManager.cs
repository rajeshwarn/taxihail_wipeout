using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class TemporaryPaymentInfoManager :
        IIntegrationEventHandler,
        IEventHandler<OrderCancelled>,
        IEventHandler<OrderCancelledBecauseOfError>,
        IEventHandler<OrderStatusChanged>,
        IEventHandler<OrderUnpairedForPayment>,
        IEventHandler<OrderSwitchedToNextDispatchCompany>

    {
        private readonly Func<BookingDbContext> _contextFactory;

        public TemporaryPaymentInfoManager(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }


        public void Handle(OrderStatusChanged @event)
        {
            if (@event.IsCompleted)
            {
                using (var context = _contextFactory.Invoke())
                {
                    RemoveTemporaryPaymentInfo(context, @event.SourceId);
                }
            }
        }

        public void Handle(OrderSwitchedToNextDispatchCompany @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                RemoveTemporaryPaymentInfo(context, @event.SourceId);
            }
        }

        public void Handle(OrderUnpairedForPayment @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                RemoveTemporaryPaymentInfo(context, @event.SourceId);
            }
        }

        public void Handle(OrderCancelledBecauseOfError @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                RemoveTemporaryPaymentInfo(context, @event.SourceId);
            }
        }

        public void Handle(OrderCancelled @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                RemoveTemporaryPaymentInfo(context, @event.SourceId);
            }
        }

        // TODO remove this once CMT has real preauth
        private void RemoveTemporaryPaymentInfo(BookingDbContext context, Guid orderId)
        {
            context.RemoveWhere<TemporaryOrderPaymentInfoDetail>(c => c.OrderId == orderId);
            context.SaveChanges();
        }
    }
}
