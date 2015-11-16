using System;
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
using apcurium.MK.Booking.Projections;

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
        IEventHandler<AutoTipUpdated>,
        IEventHandler<OriginalEtaLogged>,
        IEventHandler<OrderNotificationDetailUpdated>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly IProjectionSet<OrderDetail> _orderDetailProjectionSet;
        private readonly IProjectionSet<OrderStatusDetail> _orderStatusProjectionSet;
        private readonly ILogger _logger;
        private readonly IServerSettings _serverSettings;
        private readonly Resources.Resources _resources;

        public OrderGenerator(Func<BookingDbContext> contextFactory,
            IProjectionSet<OrderDetail> orderDetailProjectionSet,
            IProjectionSet<OrderStatusDetail> orderStatusProjectionSet,
            ILogger logger,
            IServerSettings serverSettings)
        {
            _contextFactory = contextFactory;
            _orderDetailProjectionSet = orderDetailProjectionSet;
            _orderStatusProjectionSet = orderStatusProjectionSet;
            _logger = logger;
            _serverSettings = serverSettings;
            _resources = new Resources.Resources(serverSettings);
        }

        public void Handle(OrderCancelled @event)
        {
            string clientLanguageCode = "en";
            if (_orderDetailProjectionSet.Exists(@event.SourceId))
            {
                _orderDetailProjectionSet.Update(@event.SourceId, order =>
                {
                    order.Status = (int)OrderStatus.Canceled;
                    // To update order details status description
                    clientLanguageCode = order.ClientLanguageCode;
                });
            }

            if (_orderStatusProjectionSet.Exists(@event.SourceId))
            {
                _orderStatusProjectionSet.Update(@event.SourceId, details =>
                {
                    details.Status = OrderStatus.Canceled;
                    details.IBSStatusId = VehicleStatuses.Common.CancelledDone;
                    details.IBSStatusDescription = _resources.Get("OrderStatus_wosCANCELLED", clientLanguageCode);
                });
            }

            using (var context = _contextFactory.Invoke())
            {
                RemoveTemporaryPaymentInfo(context, @event.SourceId);
            }
        }

        public void Handle(OrderCancelledBecauseOfError @event)
        {
            
            if (_orderDetailProjectionSet.Exists(@event.SourceId))
            {
                _orderDetailProjectionSet.Update(@event.SourceId, order =>
                {
                    order.Status = (int)OrderStatus.Canceled;
                });
            }

            
            if (_orderStatusProjectionSet.Exists(@event.SourceId))
            {
                _orderStatusProjectionSet.Update(@event.SourceId, details =>
                {
                    details.Status = OrderStatus.Canceled;
                    details.IBSStatusId = VehicleStatuses.Common.CancelledDone;
                    details.IBSStatusDescription = @event.ErrorDescription;
                });
            }

            using (var context = _contextFactory.Invoke())
            {
                RemoveTemporaryPaymentInfo(context, @event.SourceId);
            }
        }

        public void Handle(OrderCreated @event)
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
                Market = @event.Market,
                BookingFees = @event.BookingFees,
                TipIncentive = @event.TipIncentive
            };

            if (@event.IsPrepaid)
            {
                // NB: There could be a race condition for CC prepaid orders because the payment is done before creating the order
                // so we make sure that the order is created properly
                if (_orderDetailProjectionSet.Exists(@event.SourceId))
                {
                    // CreditCardPaymentCaptured was triggered before OrderCreated, so a basic order object was created
                    // in the CreditCardPaymentDetailsGenerator. Here, we update its values
                    _orderDetailProjectionSet.Update(@event.SourceId, order =>
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
                        order.BookingFees = @event.BookingFees;
                        order.TipIncentive = @event.TipIncentive;
                    });
                }
                else
                {
                    // OrderCreated was triggered before CreditCardPaymentCaptured so order doesn't exist yet: create it
                    _orderDetailProjectionSet.Add(orderDetail);
                }
            }
            else
            {
                // Normal flow
                _orderDetailProjectionSet.Add(orderDetail);
            }

                // Create an empty OrderStatusDetail row
            var detailsExists = _orderStatusProjectionSet.Exists(@event.SourceId);
            if (detailsExists)
            {
                _logger.LogMessage("Order Status already existing for Order : " + @event.SourceId);
            }
            else
            {
                _orderStatusProjectionSet.Add(new OrderStatusDetail
                {
                    OrderId = @event.SourceId,
                    AccountId = @event.AccountId,
                    IBSOrderId = @event.IBSOrderId,
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

                    // Unpair only available if automatic pairing is disabled
                    _orderStatusProjectionSet.Update(@event.SourceId, orderStatus =>
                    {
                        var paymentSettings = _serverSettings.GetPaymentSettings(orderStatus.CompanyKey);
                        if (!paymentSettings.IsUnpairingDisabled)
                        {
                            orderStatus.UnpairingTimeOut = paymentSettings.UnpairingTimeOut == 0
                                    ? DateTime.MaxValue                                                 // Unpair will be available for the duration of the ride
                                    : @event.EventDate.AddSeconds(paymentSettings.UnpairingTimeOut);    // Unpair will be available until timeout reached
                        }
                    });
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
						AccountId = @event.AccountId,
                        OrderId = @event.SourceId,
                        Score = ratingScore.Score,
                        RatingTypeId = ratingScore.RatingTypeId,
                        Name = ratingScore.Name
                    });
                }

                context.SaveChanges();
            }

            _orderDetailProjectionSet.Update(@event.SourceId, order =>
            {
                order.IsRated = true;
            });
        }

        public void Handle(OrderRemovedFromHistory @event)
        {
            _orderDetailProjectionSet.Update(@event.SourceId, order => 
            {
                order.IsRemovedFromHistory = true;
                order.Status = (int)OrderStatus.Removed;
            });

            if (_orderStatusProjectionSet.Exists(@event.SourceId))
            {
                _orderStatusProjectionSet.Update(@event.SourceId, details =>
                {
                    details.Status = OrderStatus.Removed;
                });
            }

        }

        private bool GetFareAvailable(double? fare)
        {
            return fare.HasValue && fare > 0;
        }

        public void Handle(OrderStatusChanged @event)
        {
            
            var fareAvailable = GetFareAvailable(@event.Fare);

            var detailsExists = _orderStatusProjectionSet.Exists(@event.SourceId);
            if (!detailsExists)
            {
                @event.Status.NetworkPairingTimeout = GetNetworkPairingTimeoutIfNecessary(@event.Status, @event.EventDate);

                @event.Status.FareAvailable = fareAvailable;
                _orderStatusProjectionSet.Add(@event.Status);
            }
            else
            {
                _orderStatusProjectionSet.Update(@event.SourceId, details =>
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
                        details.TaxiAssignedDate = @event.Status.TaxiAssignedDate;
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
                });
            }
            
            if (_orderDetailProjectionSet.Exists(@event.SourceId))
            {
                _orderDetailProjectionSet.Update(@event.SourceId, order =>
                {
                    // possible only with migration from OrderCompleted or OrderFareUpdated
                    if (@event.Status == null)
                    {
                        if (@event.IsCompleted)
                        {
                            order.Status = (int)OrderStatus.Completed;
                        }
                    }
                    else
                    {
                        order.Status = (int)@event.Status.Status;
                    }

                    if (@event.IsCompleted)
                    {
                        order.DropOffDate = @event.EventDate;
                    }

                    order.Fare = @event.Fare;
                    order.Tip = @event.Tip;
                    order.Toll = @event.Toll;
                    order.Tax = @event.Tax;
                    order.Surcharge = @event.Surcharge;

                });

                if (@event.IsCompleted)
                {
                    using (var context = _contextFactory.Invoke())
                    {
                        RemoveTemporaryPaymentInfo(context, @event.SourceId);
                    }
                }
            }
            else
            {
                _logger.LogMessage("Order Status without existing Order : " + @event.SourceId);
            }
        }

        public void Handle(OrderUnpairedForPayment @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderPairingDetail = context.Find<OrderPairingDetail>(@event.SourceId);
                if (orderPairingDetail != null)
                {
                    //context.Set<OrderPairingDetail>().Remove(orderPairingDetail);
                    orderPairingDetail.WasUnpaired = true;
                    context.Save(orderPairingDetail);
                }

                RemoveTemporaryPaymentInfo(context, @event.SourceId);
            }

            _orderDetailProjectionSet.Update(@event.SourceId, order =>
            {
                order.Settings.ChargeTypeId = ChargeTypes.PaymentInCar.Id;
                order.Settings.ChargeType = ChargeTypes.PaymentInCar.Display;
            });

        }

        public void Handle(PaymentInformationSet @event)
        {
            _orderDetailProjectionSet.Update(@event.SourceId, order =>
            {
                order.PaymentInformation.PayWithCreditCard = true;
                order.PaymentInformation.CreditCardId = @event.CreditCardId;
                order.PaymentInformation.TipAmount = @event.TipAmount;
                order.PaymentInformation.TipPercent = @event.TipPercent;
            });
        }

        public void Handle(OrderPreparedForNextDispatch @event)
        {
            _orderStatusProjectionSet.Update(@event.SourceId, details =>
            {
                details.Status = OrderStatus.TimedOut;
                details.NextDispatchCompanyName = @event.DispatchCompanyName;
                details.NextDispatchCompanyKey = @event.DispatchCompanyKey;
            });

        }

        public void Handle(OrderSwitchedToNextDispatchCompany @event)
        {
            string clientLanguageCode = null;
            _orderDetailProjectionSet.Update(@event.SourceId, order =>
            {
                clientLanguageCode = order.ClientLanguageCode;

                order.Status = (int)OrderStatus.Created;
                order.IBSOrderId = @event.IBSOrderId;
                order.CompanyKey = @event.CompanyKey;
                order.CompanyName = @event.CompanyName;
                order.Market = @event.Market;
            });
            
            _orderStatusProjectionSet.Update(@event.SourceId, details => 
            {
                details.Status = OrderStatus.Created;
                details.IBSStatusId = null;             //set it to null to trigger an update in OrderStatusUpdater
                details.IBSStatusDescription = string.Format(_resources.Get("OrderStatus_wosWAITINGRoaming", clientLanguageCode), @event.CompanyName);
                details.IBSOrderId = @event.IBSOrderId;
                details.CompanyKey = @event.CompanyKey;
                details.CompanyName = @event.CompanyName;
                details.Market = @event.Market;
                details.NextDispatchCompanyKey = null;
                details.NextDispatchCompanyName = null;
                details.NetworkPairingTimeout = GetNetworkPairingTimeoutIfNecessary(details, @event.EventDate);
            });

            using (var context = _contextFactory.Invoke())
            {
                RemoveTemporaryPaymentInfo(context, @event.SourceId);
            }
        }

        public void Handle(DispatchCompanySwitchIgnored @event)
        {
            _orderStatusProjectionSet.Update(@event.SourceId, details =>
            {
                details.IgnoreDispatchCompanySwitch = true;
                details.Status = OrderStatus.Created;
                details.NextDispatchCompanyKey = null;
                details.NextDispatchCompanyName = null;
            });
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
            _orderDetailProjectionSet.Update(@event.SourceId, order =>
            {
                order.IBSOrderId = @event.IBSOrderId;
            });

            _orderStatusProjectionSet.Update(@event.SourceId, orderStatus =>
            {
                orderStatus.IBSOrderId = @event.IBSOrderId;
            });
        }

        public void Handle(OrderManuallyPairedForRideLinq @event)
        {
            _orderDetailProjectionSet.Add(new OrderDetail
            {
                AccountId = @event.AccountId,
                Id = @event.SourceId,
                IBSOrderId = @event.TripId,
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
            var detailsExists = _orderStatusProjectionSet.Exists(@event.SourceId);
            if (detailsExists)
            {
                _logger.LogMessage("Order Status already existing for Order : " + @event.SourceId);
            }
            else
            {
                _orderStatusProjectionSet.Add(new OrderStatusDetail
                {
                    OrderId = @event.SourceId,
                    AccountId = @event.AccountId,
                    Status = OrderStatus.Created,
                    IBSStatusDescription = _resources.Get("CreateOrder_WaitingForIbs", @event.ClientLanguageCode),
                    PickupDate = @event.PairingDate,
                    IsManualRideLinq = true,
                    VehicleNumber = @event.Medallion,
                    DriverInfos = new DriverInfos
                    {
                        DriverId = @event.DriverId.ToString()
                    }
                });
            }

            using (var context = _contextFactory.Invoke())
            {
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
						DeviceName = @event.DeviceName,
                        TripId = @event.TripId,
                        DriverId = @event.DriverId,
                        LastFour = @event.LastFour,
                        AccessFee = @event.AccessFee
                    });
                }
            }
        }

        public void Handle(OrderUnpairedFromManualRideLinq @event)
        {
            if(_orderDetailProjectionSet.Exists(@event.SourceId))
            {
                _orderDetailProjectionSet.Update(@event.SourceId, order =>
                {
                    order.Status = (int)OrderStatus.Canceled;
                });
            }

            if(_orderStatusProjectionSet.Exists(@event.SourceId))
            {
                _orderStatusProjectionSet.Update(@event.SourceId, orderStatusDetails =>
                {
                    orderStatusDetails.Status = OrderStatus.Canceled;
                });
            }

            using (var context = _contextFactory.Invoke())
            {
                var rideLinqDetails = context.Find<OrderManualRideLinqDetail>(@event.SourceId);
                if (rideLinqDetails != null)
                {
                    // Must set an endtime to end order on client side
                    rideLinqDetails.EndTime = DateTime.UtcNow;
                    rideLinqDetails.IsCancelled = true;
                    
                    context.Save(rideLinqDetails);
                }
            }
        }

        public void Handle(ManualRideLinqTripInfoUpdated @event)
        {
            _logger.LogMessage("Trip info updated event received for order {0} (TripId {1}; Pairing token {2}", @event.SourceId, @event.TripId, @event.PairingToken);
 
            if (_orderDetailProjectionSet.Exists(@event.SourceId))
            {
                _orderDetailProjectionSet.Update(@event.SourceId, order =>
                {
                    if (@event.EndTime.HasValue)
                    {
                        order.Status = (int)OrderStatus.Completed;
                        order.DropOffDate = @event.EndTime;
                    }
                    else if (@event.PairingError.HasValueTrimmed())
                    {
                        order.Status = (int)OrderStatus.TimedOut;
                    }

                    order.Fare = @event.Fare;
                    order.Tax = @event.Tax;
                    order.Toll = @event.Toll;
                    order.Tip = @event.Tip;
                });
            }

            if (_orderStatusProjectionSet.Exists(@event.SourceId))
            {
                _orderStatusProjectionSet.Update(@event.SourceId, orderStatusDetails =>
                {
                    if (@event.EndTime.HasValue)
                    {
                        orderStatusDetails.Status = OrderStatus.Completed;
                    }
                    else if (@event.PairingError.HasValueTrimmed())
                    {
                        orderStatusDetails.Status = OrderStatus.TimedOut;
                    }
                });
            }

            using (var context = _contextFactory.Invoke())
            {
                var rideLinqDetails = context.Find<OrderManualRideLinqDetail>(@event.SourceId);
                if (rideLinqDetails == null)
                {
                    _logger.LogMessage("There is no manual RideLinQ details for order {0}", @event.SourceId);
                    return;
                }

                rideLinqDetails.DriverId = @event.DriverId;
                rideLinqDetails.StartTime = @event.StartTime;
                rideLinqDetails.EndTime = @event.EndTime;
                rideLinqDetails.TripId = @event.TripId;
                rideLinqDetails.Distance = @event.Distance;
                rideLinqDetails.PairingToken = @event.PairingToken;
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
                rideLinqDetails.AccessFee = @event.AccessFee;
                rideLinqDetails.LastFour = @event.LastFour;
                rideLinqDetails.PairingError = @event.PairingError;

                context.Save(rideLinqDetails);
            }
        }

        public void Handle(AutoTipUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderPairing = context.Find<OrderPairingDetail>(@event.SourceId);
                if (orderPairing == null)
                {
                    _logger.LogMessage("No Pairing found for Order : " + @event.SourceId);
                    return;
                }

                orderPairing.AutoTipPercentage = @event.AutoTipPercentage;
                context.Save(orderPairing);
            }
        }

        public void Handle(OriginalEtaLogged @event)
        {
            if(!_orderStatusProjectionSet.Exists(@event.SourceId))
            {
                return;
            }

            _orderStatusProjectionSet.Update(@event.SourceId, orderStatus =>
            {
                if (!orderStatus.OriginalEta.HasValue)
                {
                    orderStatus.OriginalEta = @event.OriginalEta;
                }
            });
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