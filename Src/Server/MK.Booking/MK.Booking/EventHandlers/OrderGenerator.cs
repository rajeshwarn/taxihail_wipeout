#region

using System;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.SqlTypes;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using AutoMapper;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Common.Configuration;

#endregion

namespace apcurium.MK.Booking.EventHandlers
{
    public class OrderGenerator : IEventHandler<OrderCreated>,
        IEventHandler<OrderCancelled>,
        IEventHandler<OrderCompleted>,
        IEventHandler<OrderRemovedFromHistory>,
        IEventHandler<OrderRated>,
        IEventHandler<PaymentInformationSet>,
        IEventHandler<OrderStatusChanged>,
        IEventHandler<OrderVehiclePositionChanged>,
        IEventHandler<OrderPairedForRideLinqCmtPayment>,
        IEventHandler<OrderUnpairedForRideLinqCmtPayment>,
        IEventHandler<OrderFareUpdated>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly ILogger _logger;
        private readonly Resources.Resources _resources;

        public OrderGenerator(Func<BookingDbContext> contextFactory, ILogger logger, IConfigurationManager configurationManager)
        {
            _contextFactory = contextFactory;
            _logger = logger;

            var applicationKey = configurationManager.GetSetting("TaxiHail.ApplicationKey");
            _resources = new Resources.Resources(applicationKey);
        }

        public void Handle(OrderCancelled @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(@event.SourceId);
                order.Status = (int) OrderStatus.Canceled;
                context.Save(order);

                var details = context.Find<OrderStatusDetail>(@event.SourceId);
                if (details != null)
                {
                    details.Status = OrderStatus.Canceled;
                    details.IBSStatusId = VehicleStatuses.Common.CancelledDone;
                    details.IBSStatusDescription = "Order Cancelled";
                    context.Save(details);
                }
            }
        }

        public void Handle(OrderCompleted @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(@event.SourceId);
                order.Status = (int) OrderStatus.Completed;
                order.Fare = @event.Fare;
                order.Tip = @event.Tip;
                order.Toll = @event.Toll;
                order.Tax = @event.Tax;
                context.Save(order);
            }
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
                    Status = (int) OrderStatus.Created,
                    IsRated = false,
                    EstimatedFare = @event.EstimatedFare,
                    UserAgent = @event.UserAgent,
                    ClientLanguageCode = @event.ClientLanguageCode,
                    ClientVersion = @event.ClientVersion
                });

                // Create an empty OrderStatusDetail row
                var details = context.Find<OrderStatusDetail>(@event.SourceId);
                if (details != null)
                {
                    _logger.LogMessage("Order Status already existing for Order : " + @event.SourceId);
                }
                else
                {
                    context.Save(new OrderStatusDetail
                    {
                        OrderId = @event.SourceId,
                        AccountId = @event.AccountId,
                        IBSOrderId  = @event.IBSOrderId,
                        Status = OrderStatus.Created,
                        IBSStatusDescription = (string)_resources.Get("OrderStatus_wosWAITING", @event.ClientLanguageCode),
                        PickupDate = @event.PickupDate,
                        Name = @event.Settings != null ? @event.Settings.Name : null
                    });
                }
            }
        }

        public void Handle(OrderPairedForRideLinqCmtPayment @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Save(new OrderPairingDetail
                {
                    OrderId = @event.SourceId,
                    Medallion = @event.Medallion,
                    DriverId = @event.DriverId,
                    PairingToken = @event.PairingToken,
                    PairingCode = @event.PairingCode,
                    TokenOfCardToBeUsedForPayment = @event.TokenOfCardToBeUsedForPayment,
                    AutoTipAmount = @event.AutoTipAmount,
                    AutoTipPercentage = @event.AutoTipPercentage
                });
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
                    Mapper.Map(@event.Status, details);
                    context.Save(details);
                }

                var order = context.Find<OrderDetail>(@event.SourceId);
                if (order != null)
                {
                    order.Status = (int) @event.Status.Status;
                    context.Save(order);
                }
                else
                {
                    _logger.LogMessage("Order Status without existing Order : " + @event.SourceId);
                }

                context.SaveChanges();
            }
        }

        public void Handle(OrderFareUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var details = context.Find<OrderStatusDetail>(@event.SourceId);
                details.FareAvailable = true;
                context.Save(details);

                var order = context.Find<OrderDetail>(@event.SourceId);
                if (order != null)
                {
                    order.Fare = @event.Fare;
                    order.Tip = @event.Tip;
                    order.Toll = @event.Toll;
                    order.Tax = @event.Tax;
                    context.Save(order);
                }
                else
                {
                    _logger.LogMessage("Order Status without existing Order : " + @event.SourceId);
                }

                context.SaveChanges();
            }
        }

        public void Handle(OrderUnpairedForRideLinqCmtPayment @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderPairingDetail = context.Find<OrderPairingDetail>(@event.SourceId);
                if (orderPairingDetail != null)
                {
                    context.Set<OrderPairingDetail>().Remove(orderPairingDetail);
                    context.SaveChanges();
                }
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
    }
}