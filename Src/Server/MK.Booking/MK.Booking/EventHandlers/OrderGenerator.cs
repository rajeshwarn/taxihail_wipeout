﻿using System;
using System.Data.SqlTypes;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Entity;


namespace apcurium.MK.Booking.EventHandlers
{
    public class OrderGenerator : IEventHandler<OrderCreated>,
        IEventHandler<OrderCancelled>,
        IEventHandler<OrderCompleted>,
        IEventHandler<OrderRemovedFromHistory>,
        IEventHandler<OrderRated>,
        IEventHandler<PaymentInformationSet>,
        IEventHandler<TransactionIdSet>,
        IEventHandler<OrderStatusChanged>,
        IEventHandler<OrderVehiclePositionChanged>
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
                    Settings = @event.Settings,
                    Status = (int)OrderStatus.Created,
                    IsRated = false
                });

                // Create an empty OrderStatusDetail row
                context.Save(new OrderStatusDetail
                {
                     OrderId = @event.SourceId,
                     AccountId = @event.AccountId,
                     IBSOrderId = @event.IBSOrderId,
                     Status = OrderStatus.Created,
                     IBSStatusDescription = "Processing your order",
                     PickupDate = @event.PickupDate,
                     Name = @event.Settings != null ? @event.Settings.Name : null
                });
            }

        }

        public void Handle(OrderCancelled @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(@event.SourceId);
                order.Status = (int)OrderStatus.Canceled;
                context.Save(order);

                var details = context.Find<OrderStatusDetail>(@event.SourceId);
                if (details != null)
                {
                    details.Status = OrderStatus.Canceled;
                    context.Save(details);
                }
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
                order.IsRemovedFromHistory = true;

                var details = context.Find<OrderStatusDetail>(@event.SourceId);
                if (details != null)
                {
                    details.Status = OrderStatus.Removed;
                    context.Save(details);
                }

                context.SaveChanges();
            }
        }

        public void Handle(OrderRated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Set<OrderRatingDetails>().Add(new OrderRatingDetails
                {
                    Id = Guid.NewGuid(),
                    OrderId = @event.SourceId,
                    Note = @event.Note,
                });

                foreach (var ratingScore in @event.RatingScores)
                {
                    context.Set<RatingScoreDetails>().Add(new RatingScoreDetails
                    {
                        Id = Guid.NewGuid(),
                        OrderId = @event.SourceId,
                        Score = ratingScore.Score,
                        RatingTypeId = ratingScore.RatingTypeId,
                        Name = ratingScore.Name
                    });
                }

                var order = context.Find<OrderDetail>(@event.SourceId);
                order.IsRated = true;

                context.SaveChanges();
            }
        }

        public void Handle(PaymentInformationSet @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(@event.SourceId);
                order.PaymentInformation.PayWithCreditCard = true;
                order.PaymentInformation.CreditCardId = @event.CreditCardId;
                order.PaymentInformation.TipAmount = @event.TipAmount;
                order.PaymentInformation.TipPercent = @event.TipPercent;

                context.Save(order);
            }
        }

        public void Handle(OrderStatusChanged @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                @event.Status.PickupDate = @event.Status.PickupDate < (DateTime) SqlDateTime.MinValue
                                               ? (DateTime) SqlDateTime.MinValue
                                               : @event.Status.PickupDate;
                var details = context.Find<OrderStatusDetail>(@event.Status.OrderId);
                if (details == null)
                {
                    context.Set<OrderStatusDetail>().Add(@event.Status);
                }
                else
                {
                    AutoMapper.Mapper.Map(@event.Status, details);
                    context.Save(details);
                }

                var order = context.Find<OrderDetail>(@event.SourceId);
                order.Status = (int)@event.Status.Status;
                context.Save(order);

                context.SaveChanges();
            }
        }
        
        public void Handle(TransactionIdSet @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(@event.SourceId);
                order.TransactionId = @event.TransactionId;

                context.Save(order);
            }
        }

        public void Handle(OrderVehiclePositionChanged @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderStatusDetail>(@event.SourceId);
                order.VehicleLatitude = @event.Latitude;
                order.VehicleLongitude = @event.Longitude;

                context.Save(order);
            }
        }
    }
}
