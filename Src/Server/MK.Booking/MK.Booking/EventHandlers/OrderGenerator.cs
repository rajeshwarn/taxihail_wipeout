using System;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.EventHandlers
{
    public class OrderGenerator : IEventHandler<OrderCreated>, IEventHandler<OrderCancelled>, IEventHandler<OrderCompleted>, IEventHandler<OrderRemovedFromHistory>
    {

        private readonly Func<BookingDbContext> _contextFactory;

        public OrderGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(OrderCreated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Save(new OrderDetail
                {
                    IBSOrderId = @event.IBSOrderId,
                    AccountId = @event.AccountId,
                    PickupAddress = @event.PickupAddress,
                    Id = @event.SourceId,
                    PickupDate = @event.PickupDate,
                    CreatedDate = @event.CreatedDate,
                    DropOffAddress = @event.DropOffAddress,
                    Status = (int)OrderStatus.Created,
                });
            }
        }

        public void Handle(OrderCancelled @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(@event.SourceId);
                order.Status = (int)OrderStatus.Cancelled;
                context.Save(order);
            }
        }

        public void Handle(OrderCompleted @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(@event.SourceId);
                order.Status = (int)OrderStatus.Completed;
                order.Fare = @event.Fare;
                order.Toll = @event.Toll;
                order.Tip = @event.Tip;
                context.Save(order);
            }
        }

        public void Handle(OrderRemovedFromHistory @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(@event.SourceId);
                context.Set<OrderDetail>().Remove(order);
                context.SaveChanges();
            }
        }
    }
}
