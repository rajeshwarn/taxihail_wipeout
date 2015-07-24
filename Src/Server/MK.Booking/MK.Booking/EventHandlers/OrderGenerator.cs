﻿using System;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class OrderGenerator : IEventHandler<OrderCreated>,
        IEventHandler<OrderCancelled>,
        IEventHandler<OrderRemovedFromHistory>,
        IEventHandler<OrderRated>,
        IEventHandler<PaymentInformationSet>,
        IEventHandler<OrderStatusChanged>,
        IEventHandler<OrderPairedForPayment>,
        IEventHandler<OrderUnpairedForPayment>,
        IEventHandler<OrderPreparedForNextDispatch>,
        IEventHandler<OrderSwitchedToNextDispatchCompany>,
        IEventHandler<DispatchCompanySwitchIgnored>,
        IEventHandler<IbsOrderInfoAddedToOrder>,
        IEventHandler<OrderCancelledBecauseOfError>,
        IEventHandler<OrderManuallyPairedForRideLinq>,
        IEventHandler<OrderUnpairedFromManualRideLinq>,
        IEventHandler<ManualRideLinqTripInfoUpdated>,
        IEventHandler<OrderNotificationDetailUpdated>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly ILogger _logger;
        private readonly IServerSettings _serverSettings;
        private readonly Resources.Resources _resources;
        public OrderGenerator(Func<BookingDbContext> contextFactory,
            ILogger logger,
            IServerSettings serverSettings)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            _serverSettings = serverSettings;
            _resources = new Resources.Resources(serverSettings);
        }

        public void Handle(OrderCancelled @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(@event.SourceId);
                if (order != null)
                {
                    order.Status = (int) OrderStatus.Canceled;
                    context.Save(order);
                }

                var details = context.Find<OrderStatusDetail>(@event.SourceId);
                if (details != null)
                {
                    details.Status = OrderStatus.Canceled;
                    details.IBSStatusId = VehicleStatuses.Common.CancelledDone;
                    details.IBSStatusDescription = _resources.Get("OrderStatus_wosCANCELLED", order != null ? order.ClientLanguageCode : "en");
                    context.Save(details);
                }

                RemoveTemporaryPaymentInfo(context, @event.SourceId);
            }
        }

        public void Handle(OrderCancelledBecauseOfError @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(@event.SourceId);
                if (order != null)
                {
                    order.Status = (int)OrderStatus.Canceled;
                    context.Save(order);
                }

                var details = context.Find<OrderStatusDetail>(@event.SourceId);
                if (details != null)
                {
                    details.Status = OrderStatus.Canceled;
                    details.IBSStatusId = VehicleStatuses.Common.CancelledDone;
                    details.IBSStatusDescription = @event.ErrorDescription;
                    context.Save(details);
                }

                RemoveTemporaryPaymentInfo(context, @event.SourceId);
            }
        }

        public void Handle(OrderCreated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderDetail = new OrderDetail
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
                    IsRated = false,
                    EstimatedFare = @event.EstimatedFare,
                    UserAgent = @event.UserAgent,
                    UserNote = @event.UserNote,
                    ClientLanguageCode = @event.ClientLanguageCode,
                    ClientVersion = @event.ClientVersion,
                    CompanyKey = @event.CompanyKey,
                    CompanyName = @event.CompanyName,
                    Market = @event.Market
                };

                if (@event.IsPrepaid)
                {
                    // NB: There could be a race condition for CC prepaid orders because the payment is done before creating the order
                    // so we make sure that the order is created properly
                    var order = context.Find<OrderDetail>(@event.SourceId);

                    // CreditCardPaymentCaptured was triggered before OrderCreated, so a basic order object was created
                    // in the CreditCardPaymentDetailsGenerator. Here, we update its values
                    if (order != null)
                    {
                        order.IBSOrderId = @event.IBSOrderId;
                        order.AccountId = @event.AccountId;
                        order.PickupAddress = @event.PickupAddress;
                        order.PickupDate = @event.PickupDate;
                        order.CreatedDate = @event.CreatedDate;
                        order.DropOffAddress = @event.DropOffAddress;
                        order.Settings = @event.Settings;
                        order.Status = (int)OrderStatus.Created;
                        order.IsRated = false;
                        order.EstimatedFare = @event.EstimatedFare;
                        order.UserAgent = @event.UserAgent;
                        order.UserNote = @event.UserNote;
                        order.ClientLanguageCode = @event.ClientLanguageCode;
                        order.ClientVersion = @event.ClientVersion;
                        order.CompanyKey = @event.CompanyKey;
                        order.CompanyName = @event.CompanyName;
                        order.Market = @event.Market;

                        context.SaveChanges();
                    }
                    else
                    {
                        // OrderCreated was triggered before CreditCardPaymentCaptured so order doesn't exist yet: create it
                        context.Save(orderDetail);
                    }
                }
                else
                {
                    // Normal flow
                    context.Save(orderDetail);
                }

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
                        IBSStatusDescription = _resources.Get("CreateOrder_WaitingForIbs", @event.ClientLanguageCode),
                        PickupDate = @event.PickupDate,
                        Name = @event.Settings != null ? @event.Settings.Name : null,
                        IsChargeAccountPaymentWithCardOnFile = @event.IsChargeAccountPaymentWithCardOnFile,
                        IsPrepaid = @event.IsPrepaid,
                        CompanyKey = @event.CompanyKey,
                        CompanyName = @event.CompanyName,
                        Market = @event.Market
                    });
                }
            }
        }

        public void Handle(OrderPairedForPayment @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var existingPairing = context.Find<OrderPairingDetail>(@event.SourceId);
                if (existingPairing != null)
                {
                    _logger.LogMessage("Order Pairing already existing for Order : " + @event.SourceId);
                }
                else
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

                    var paymentSettings = _serverSettings.GetPaymentSettings();
                    if (!paymentSettings.IsUnpairingDisabled)
                    {
                        // Unpair only available if automatic pairing is disabled
                        var orderStatus = context.Find<OrderStatusDetail>(@event.SourceId);
                        orderStatus.UnpairingTimeOut = @event.EventDate.AddSeconds(paymentSettings.UnpairingTimeOut);
                        context.Save(orderStatus);
                    }
                }
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
                order.Status = (int)OrderStatus.Removed;

                var details = context.Find<OrderStatusDetail>(@event.SourceId);
                if (details != null)
                {
                    details.Status = OrderStatus.Removed;
                    context.Save(details);
                }

                context.SaveChanges();
            }
        }

        private bool GetFareAvailable(double? fare)
        {
            return fare.HasValue && fare > 0;
        }

        public void Handle(OrderStatusChanged @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var fareAvailable = GetFareAvailable(@event.Fare);

                var details = context.Find<OrderStatusDetail>(@event.SourceId);
                if (details == null)
                {
                    @event.Status.NetworkPairingTimeout = GetNetworkPairingTimeoutIfNecessary(@event.Status, @event.EventDate);

                    @event.Status.FareAvailable = fareAvailable;
                    context.Set<OrderStatusDetail>().Add(@event.Status);
                }
                else
                {
                    if (@event.Status != null) 
                    {
                        details.NetworkPairingTimeout = GetNetworkPairingTimeoutIfNecessary(@event.Status, @event.EventDate);

                        details.IBSStatusId = @event.Status.IBSStatusId;
                        details.DriverInfos = @event.Status.DriverInfos;
                        details.VehicleNumber = @event.Status.VehicleNumber;
                        details.TerminalId = @event.Status.TerminalId;
                        details.ReferenceNumber = @event.Status.ReferenceNumber;
                        details.Eta = @event.Status.Eta;
                        details.Status = @event.Status.Status;
                        details.IBSStatusDescription = @event.Status.IBSStatusDescription;
                        details.PairingTimeOut = @event.Status.PairingTimeOut;
                        details.PairingError = @event.Status.PairingError;
                        details.RideLinqPairingCode = @event.Status.RideLinqPairingCode;
                    }
                    else
                    {
                        // it will enter here only with migration from OrderCompleted or OrderFareUpdated
                        if (@event.IsCompleted)
                        {
                            details.Status = OrderStatus.Completed;
                        }
                    }

                    details.FareAvailable = fareAvailable;
                    context.Save(details);
                }

                var order = context.Find<OrderDetail>(@event.SourceId);
                if (order != null)
                {
                    // possible only with migration from OrderCompleted or OrderFareUpdated
                    if (@event.Status == null) 
                    {
                        if (@event.IsCompleted)
                        {
                            order.Status = (int) OrderStatus.Completed;
                        }
                    }
                    else
                    {
                        order.Status = (int)@event.Status.Status;
                    }

                    if (@event.IsCompleted)
                    {
                        RemoveTemporaryPaymentInfo(context, @event.SourceId);

                        order.DropOffDate = @event.EventDate;
                    }

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

        public void Handle(OrderUnpairedForPayment @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderPairingDetail = context.Find<OrderPairingDetail>(@event.SourceId);
                if (orderPairingDetail != null)
                {
                    context.Set<OrderPairingDetail>().Remove(orderPairingDetail);
                    context.Save(orderPairingDetail);
                }

                var orderDetail = context.Find<OrderDetail>(@event.SourceId);
                orderDetail.Settings.ChargeTypeId = ChargeTypes.PaymentInCar.Id;
                orderDetail.Settings.ChargeType = ChargeTypes.PaymentInCar.Display;
                context.Save(orderDetail);

                RemoveTemporaryPaymentInfo(context, @event.SourceId);
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

        public void Handle(OrderPreparedForNextDispatch @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var details = context.Find<OrderStatusDetail>(@event.SourceId);
                details.Status = OrderStatus.TimedOut;
                details.NextDispatchCompanyName = @event.DispatchCompanyName;
                details.NextDispatchCompanyKey = @event.DispatchCompanyKey;

                context.Save(details);
            }
        }

        public void Handle(OrderSwitchedToNextDispatchCompany @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(@event.SourceId);
                order.Status = (int)OrderStatus.Created;
                order.IBSOrderId = @event.IBSOrderId;
                order.CompanyKey = @event.CompanyKey;
                order.CompanyName = @event.CompanyName;
                order.Market = @event.Market;

                var details = context.Find<OrderStatusDetail>(@event.SourceId);
                details.Status = OrderStatus.Created;
                details.IBSStatusId = null;             //set it to null to trigger an update in OrderStatusUpdater
                details.IBSStatusDescription = string.Format(_resources.Get("OrderStatus_wosWAITINGRoaming", order.ClientLanguageCode), @event.CompanyName);
                details.IBSOrderId = @event.IBSOrderId;
                details.CompanyKey = @event.CompanyKey;
                details.CompanyName = @event.CompanyName;
                details.Market = @event.Market;
                details.NextDispatchCompanyKey = null;
                details.NextDispatchCompanyName = null;
                details.NetworkPairingTimeout = GetNetworkPairingTimeoutIfNecessary(details, @event.EventDate);

                context.SaveChanges();

                RemoveTemporaryPaymentInfo(context, @event.SourceId);
            }
        }

        public void Handle(DispatchCompanySwitchIgnored @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var details = context.Find<OrderStatusDetail>(@event.SourceId);
                details.IgnoreDispatchCompanySwitch = true;
                details.Status = OrderStatus.Created;
                details.NextDispatchCompanyKey = null;
                details.NextDispatchCompanyName = null;

                context.Save(details);
            }
        }

        private DateTime? GetNetworkPairingTimeoutIfNecessary(OrderStatusDetail details, DateTime eventDate)
        {
            if (details.IBSStatusId.SoftEqual(VehicleStatuses.Common.Waiting)
                            && !details.NetworkPairingTimeout.HasValue
                            && _serverSettings.ServerData.Network.Enabled)
            {
                if (!details.CompanyKey.HasValue()
                    || (details.Market.HasValue() && !details.NextDispatchCompanyKey.HasValue()))
                {
                    // First timeout
                    return eventDate.AddSeconds(_serverSettings.ServerData.Network.PrimaryOrderTimeout);
                }
                // Subsequent timeouts
                return eventDate.AddSeconds(_serverSettings.ServerData.Network.SecondaryOrderTimeout);
            }
            return null;
        }

        public void Handle(IbsOrderInfoAddedToOrder @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(@event.SourceId);
                order.IBSOrderId = @event.IBSOrderId;

                var orderStatus = context.Find<OrderStatusDetail>(@event.SourceId);
                orderStatus.IBSOrderId = @event.IBSOrderId;
                
                context.SaveChanges();
            }
        }

        public void Handle(OrderManuallyPairedForRideLinq @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Save(new OrderDetail
                {
                    AccountId = @event.AccountId,
                    Id = @event.SourceId,
                    PickupDate = @event.PairingDate,
                    CreatedDate = @event.PairingDate,
                    PickupAddress = @event.PickupAddress,
                    Status = (int)OrderStatus.Created,
                    IsRated = false,
                    UserAgent = @event.UserAgent,
                    ClientLanguageCode = @event.ClientLanguageCode,
                    ClientVersion = @event.ClientVersion,
                    IsManualRideLinq = true
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
                        Status = OrderStatus.Created,
                        IBSStatusDescription = _resources.Get("CreateOrder_WaitingForIbs", @event.ClientLanguageCode),
                        PickupDate = @event.PairingDate,
                        IsManualRideLinq = true
                    });
                }

                var rideLinqDetails = context.Find<OrderManualRideLinqDetail>(@event.SourceId);
                if (rideLinqDetails != null)
                {
                    _logger.LogMessage("RideLinqDetails already existing for Order : " + @event.SourceId);
                }
                else
                {
                    context.Save(new OrderManualRideLinqDetail
                    {
                        OrderId = @event.SourceId,
                        AccountId = @event.AccountId,
                        PairingCode = @event.PairingCode,
                        PairingToken = @event.PairingToken,
                        PairingDate = @event.PairingDate,
                        Distance = @event.Distance,
                        Extra = @event.Extra,
                        Fare = @event.Fare,
                        FareAtAlternateRate = @event.FareAtAlternateRate,
                        Total = @event.Total,
                        Toll = @event.Toll,
                        Tax = @event.Tax,
                        Tip = @event.Tip,
                        Surcharge = @event.Surcharge,
                        RateAtTripStart = @event.RateAtTripStart,
                        RateAtTripEnd = @event.RateAtTripEnd,
                        RateChangeTime = @event.RateChangeTime,
                        Medallion = @event.Medallion,
                        TripId = @event.TripId,
                        DriverId = @event.DriverId
                    });
                }
            }
        }

        public void Handle(OrderUnpairedFromManualRideLinq @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(@event.SourceId);
                if (order != null)
                {
                    order.Status = (int)OrderStatus.Canceled;
                    context.Save(order);
                }

                var orderStatusDetails = context.Find<OrderStatusDetail>(@event.SourceId);
                if (orderStatusDetails != null)
                {
                    orderStatusDetails.Status = OrderStatus.Canceled;
                    context.Save(orderStatusDetails);
                }

                var rideLinqDetails = context.Find<OrderManualRideLinqDetail>(@event.SourceId);
                if (rideLinqDetails != null)
                {
                    rideLinqDetails.IsCancelled = true;
                    context.Save(rideLinqDetails);
                }
            }
        }

        public void Handle(ManualRideLinqTripInfoUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {   
                _logger.LogMessage("Trip info updated event received for order {0} (TripId {1}; Pairing token {2}", @event.SourceId, @event.TripId, @event.PairingToken);
                var order = context.Find<OrderDetail>(@event.SourceId);
                if (order != null)
                {
                    if (@event.EndTime.HasValue)
                    {
                        order.Status = (int) OrderStatus.Completed;
                        order.DropOffDate = @event.EndTime;
                    }
                    order.Fare = @event.Fare;
                    order.Tax = @event.Tax;
                    order.Toll = @event.Toll;
                    order.Tip = @event.Tip;
                    context.Save(order);
                }

                var orderStatusDetails = context.Find<OrderStatusDetail>(@event.SourceId);
                if (orderStatusDetails != null)
                {
                    if (@event.EndTime.HasValue)
                    {
                        orderStatusDetails.Status = OrderStatus.Completed;
                    }
                    context.Save(orderStatusDetails);
                }

                var rideLinqDetails = context.Find<OrderManualRideLinqDetail>(@event.SourceId);
                if (rideLinqDetails == null)
                {
                    _logger.LogMessage("There is no manual RideLinQ details for order {0}", @event.SourceId);
                    return;
                }

                rideLinqDetails.Distance = @event.Distance;
                rideLinqDetails.PairingToken = @event.PairingToken;
                rideLinqDetails.EndTime = @event.EndTime;
                rideLinqDetails.Extra = @event.Extra;
                rideLinqDetails.Fare = @event.Fare;
                rideLinqDetails.FareAtAlternateRate = @event.FareAtAlternateRate;
                rideLinqDetails.Total = @event.Total;
                rideLinqDetails.Toll = @event.Toll;
                rideLinqDetails.Tip = @event.Tip;
                rideLinqDetails.Tax = @event.Tax;
                rideLinqDetails.Surcharge = @event.Surcharge;
                rideLinqDetails.RateAtTripStart = @event.RateAtTripStart;
                rideLinqDetails.RateAtTripEnd = @event.RateAtTripEnd;
                rideLinqDetails.RateChangeTime = @event.RateChangeTime;
                context.Save(rideLinqDetails);
            }
        }

        public void Handle(OrderNotificationDetailUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderNotificationDetail = context.Find<OrderNotificationDetail>(@event.SourceId)
                    ?? new OrderNotificationDetail { Id = @event.OrderId };

                if (@event.IsTaxiNearbyNotificationSent.HasValue)
                {
                    orderNotificationDetail.IsTaxiNearbyNotificationSent = @event.IsTaxiNearbyNotificationSent.Value;
                }

                if (@event.IsUnpairingReminderNotificationSent.HasValue)
                {
                    orderNotificationDetail.IsUnpairingReminderNotificationSent = @event.IsUnpairingReminderNotificationSent.Value;
                }

                if (@event.InfoAboutPaymentWasSentToDriver.HasValue)
                {
                    orderNotificationDetail.InfoAboutPaymentWasSentToDriver = @event.InfoAboutPaymentWasSentToDriver.Value;
                }

                context.Save(orderNotificationDetail);
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