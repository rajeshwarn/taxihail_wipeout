using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;
using ServiceStack.Text;
using apcurium.MK.Booking.Projections;

namespace apcurium.MK.Booking.EventHandlers
{
    public class ReportDetailGenerator : IEventHandler<OrderCreated>,
		IEventHandler<OrderReportCreated>,
        IEventHandler<OrderStatusChanged>,
        IEventHandler<OrderCancelled>,
        IEventHandler<OrderCancelledBecauseOfError>,
        IEventHandler<OrderPairedForPayment>,
        IEventHandler<OrderUnpairedForPayment>,
        IEventHandler<OrderRated>,
        IEventHandler<CreditCardPaymentInitiated>,
        IEventHandler<CreditCardPaymentCaptured_V2>,
        IEventHandler<CreditCardErrorThrown>,
        IEventHandler<PayPalExpressCheckoutPaymentInitiated>,
        IEventHandler<PayPalExpressCheckoutPaymentCompleted>,
        IEventHandler<PayPalExpressCheckoutPaymentCancelled>,
        IEventHandler<PayPalPaymentCancellationFailed>,
        IEventHandler<PromotionApplied>,
        IEventHandler<PromotionRedeemed>,
        IEventHandler<IbsOrderInfoAddedToOrder>,
        IEventHandler<OrderSwitchedToNextDispatchCompany>,
        IEventHandler<OrderTimedOut>,
        IEventHandler<PrepaidOrderPaymentInfoUpdated>,
        IEventHandler<RefundedOrderUpdated>,
        IEventHandler<OrderManuallyPairedForRideLinq>,
        IEventHandler<OrderUnpairedFromManualRideLinq>,
        IEventHandler<ManualRideLinqTripInfoUpdated>,
        IEventHandler<OriginalEtaLogged>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly ILogger _logger;
        private readonly IProjectionSet<AccountDetail> _accountDetailProjectionSet;
        private readonly IProjectionSet<OrderReportDetail> _orderReportProjectionSet;
        private const int SQLPrimaryKeyViolationErrorNumber = 2627;

        public ReportDetailGenerator(Func<BookingDbContext> contextFactory,
            IProjectionSet<AccountDetail> accountDetailProjectionSet,
            IProjectionSet<OrderReportDetail> orderReportProjectionSet,
            ILogger logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            _accountDetailProjectionSet = accountDetailProjectionSet;
            _orderReportProjectionSet = orderReportProjectionSet;
        }

        public void Handle(OrderCreated @event)
        {
            var account = _accountDetailProjectionSet.GetProjection(@event.AccountId).Load();

            Action handling = () =>
            {
                if(!_orderReportProjectionSet.Exists(@event.SourceId))
                {
                    _orderReportProjectionSet.Add(new OrderReportDetail { Id = @event.SourceId });
                }

                _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
                {
                    orderReport.Account = new OrderReportAccount
                    {
                        AccountId = @event.AccountId,
                        Name = @event.Settings.Name,
                        Phone = @event.Settings.Phone,
                        Email = account.Email,
                        DefaultCardToken = account.DefaultCreditCard,
                        IBSAccountId = account.IBSAccountId,
                        PayBack = account.Settings.PayBack
                    };
                    orderReport.Order = new OrderReportOrder
                    {
                        IBSOrderId = @event.IBSOrderId,
                        ChargeType = @event.Settings.ChargeType,
                        IsChargeAccountPaymentWithCardOnFile = @event.IsChargeAccountPaymentWithCardOnFile,
                        IsPrepaid = @event.IsPrepaid,
                        PickupDateTime = @event.PickupDate,
                        CreateDateTime = @event.CreatedDate,
                        PickupAddress = @event.PickupAddress,
                        DropOffAddress = @event.DropOffAddress,
                        CompanyName = @event.CompanyName,
                        CompanyKey = @event.CompanyKey,
                        Market = @event.Market
                    };
                    orderReport.Client = new OrderReportClient
                    {
                        OperatingSystem = @event.UserAgent.GetOperatingSystem(),
                        UserAgent = @event.UserAgent,
                        Version = @event.ClientVersion
                    };

                });
            };

            WrapWithSqlPrimaryKeyHandling(handling);
        }

		public void Handle(OrderReportCreated @event)
		{
            var account = _accountDetailProjectionSet.GetProjection(@event.AccountId).Load();

		    Action handling = () =>
		    {
                if (!_orderReportProjectionSet.Exists(@event.SourceId))
                {
                    _orderReportProjectionSet.Add(new OrderReportDetail { Id = @event.SourceId });
                }

                _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
                {
                    orderReport.Account = new OrderReportAccount
                    {
                        AccountId = @event.AccountId,
                        Name = @event.Settings.Name,
                        Phone = @event.Settings.Phone,
                        Email = (account != null) ? account.Email : null,
                        DefaultCardToken = (account != null) ? account.DefaultCreditCard : null,
                        IBSAccountId = (account != null) ? account.IBSAccountId : null,
                        PayBack = (account != null) ? account.Settings.PayBack : null
                    };
                    orderReport.Order = new OrderReportOrder
                    {
                        IBSOrderId = @event.IBSOrderId,
                        ChargeType = @event.Settings.ChargeType,
                        IsChargeAccountPaymentWithCardOnFile = @event.IsChargeAccountPaymentWithCardOnFile,
                        IsPrepaid = @event.IsPrepaid,
                        PickupDateTime = @event.PickupDate,
                        CreateDateTime = @event.CreatedDate,
                        PickupAddress = @event.PickupAddress,
                        DropOffAddress = @event.DropOffAddress,
                        CompanyName = @event.CompanyName,
                        CompanyKey = @event.CompanyKey,
                        Market = @event.Market,
                        Error = @event.Error
                    };
                    orderReport.Client = new OrderReportClient
                    {
                        OperatingSystem = @event.UserAgent.GetOperatingSystem(),
                        UserAgent = @event.UserAgent,
                        Version = @event.ClientVersion
                    };
                });
		    };

            WrapWithSqlPrimaryKeyHandling(handling);
		}

		public void Handle(OrderStatusChanged @event)
        {
            _orderReportProjectionSet.Update(@event.SourceId, orderReport => {
                orderReport.Payment.MdtFare = @event.Fare;
                orderReport.Payment.MdtTip = @event.Tip;
                orderReport.Payment.MdtToll = @event.Toll;

                if (@event.Status != null)
                {
                    orderReport.Account.AccountId = @event.Status.AccountId;
                    orderReport.VehicleInfos.DriverFirstName = @event.Status.DriverInfos.FirstName;
                    orderReport.VehicleInfos.DriverLastName = @event.Status.DriverInfos.LastName;
                    orderReport.VehicleInfos.Number = @event.Status.VehicleNumber;
                    orderReport.VehicleInfos.Color = @event.Status.DriverInfos.VehicleColor;
                    orderReport.VehicleInfos.Make = @event.Status.DriverInfos.VehicleMake;
                    orderReport.VehicleInfos.Model = @event.Status.DriverInfos.VehicleModel;
                    orderReport.VehicleInfos.Registration = @event.Status.DriverInfos.VehicleRegistration;
                    orderReport.VehicleInfos.Type = @event.Status.DriverInfos.VehicleType;

                    orderReport.OrderStatus.Status = @event.Status.Status;

                    orderReport.Order.PickupDateTime = @event.Status.PickupDate != DateTime.MinValue
                        ? (DateTime?)@event.Status.PickupDate
                        : null;
                    orderReport.Order.CompanyName = @event.Status.CompanyName;
                }

                orderReport.OrderStatus.OrderIsCompleted = @event.IsCompleted;
            });
        }

        public void Handle(OrderPairedForPayment @event)
        {
            _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
            {
                orderReport.Payment.IsPaired = true;
            });
        }

        public void Handle(OrderUnpairedForPayment @event)
        {
            _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
            {
                orderReport.Payment.Type = null;
                orderReport.Payment.IsPaired = false;
                orderReport.Payment.WasUnpaired = true;
                orderReport.Order.ChargeType = ChargeTypes.PaymentInCar.Display;
            });
        }

        public void Handle(CreditCardPaymentInitiated @event)
        {
            Action handling = () =>
            {
                if (!_orderReportProjectionSet.Exists(@event.SourceId))
                {
                    _orderReportProjectionSet.Add(new OrderReportDetail { Id = @event.SourceId });
                }

                _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
                {
                    orderReport.Payment.PaymentId = @event.SourceId;
                    orderReport.Payment.PreAuthorizedAmount = @event.Amount;
                    orderReport.Payment.FirstPreAuthTransactionId = @event.TransactionId.ToSafeString().IsNullOrEmpty() ? "" : "Auth: " + @event.TransactionId;
                    orderReport.Payment.TransactionId = @event.TransactionId.ToSafeString().IsNullOrEmpty() ? "" : "Auth: " + @event.TransactionId;
                    orderReport.Payment.CardToken = @event.CardToken;
                    orderReport.Payment.Type = @event.Provider == PaymentProvider.PayPal ? PaymentType.PayPal : PaymentType.CreditCard;
                    orderReport.Payment.Provider = @event.Provider;
                });
            };

            WrapWithSqlPrimaryKeyHandling(handling);
        }

        public void Handle(CreditCardPaymentCaptured_V2 @event)
        {
            @event.MigrateFees();

            _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
            {
                if (@event.FeeType != FeeTypes.None)
                {
                    orderReport.Payment.TotalAmountCharged += @event.Amount;
                }
                else
                {
                    orderReport.Payment.MeterAmount = @event.Meter;
                    orderReport.Payment.TipAmount = @event.Tip;
                    orderReport.Payment.AuthorizationCode = @event.AuthorizationCode;
                    orderReport.Payment.TransactionId = @event.TransactionId.ToSafeString().IsNullOrEmpty() ? string.Empty : "Auth: " + @event.TransactionId;
                    orderReport.Payment.BookingFees = @event.BookingFees;
                }

                if (!orderReport.Payment.TotalAmountCharged.HasValue)
                {
                    orderReport.Payment.TotalAmountCharged = 0;
                }

                orderReport.Payment.TotalAmountCharged += @event.Amount;
                orderReport.Payment.IsCompleted = true;

                orderReport.Payment.WasChargedNoShowFee = @event.FeeType == FeeTypes.NoShow;
                orderReport.Payment.WasChargedCancellationFee = @event.FeeType == FeeTypes.Cancellation;
                orderReport.Payment.WasChargedBookingFee = orderReport.Payment.BookingFees > 0;
            });
        }

        public void Handle(CreditCardErrorThrown @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var payment = context.Find<OrderPaymentDetail>(@event.SourceId);
                if (payment != null)
                {
                    _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
                    {
                        orderReport.Payment.IsCancelled = true;
                        orderReport.Payment.Error = @event.Reason;
                    });
                }
            }
        }

        public void Handle(PayPalExpressCheckoutPaymentInitiated @event)
        {
            if(_orderReportProjectionSet.Exists(@event.SourceId))
            {
                _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
                {
                    orderReport.Payment.PaymentId = @event.SourceId;
                    orderReport.Payment.PayPalToken = @event.Token;
                    orderReport.Payment.TotalAmountCharged = @event.Amount;
                    orderReport.Payment.TipAmount = @event.Tip;
                    orderReport.Payment.MeterAmount = @event.Meter;
                    orderReport.Payment.Provider = PaymentProvider.PayPal;
                    orderReport.Payment.Type = PaymentType.PayPal;
                });
            }
        }

        public void Handle(PayPalExpressCheckoutPaymentCompleted @event)
        {
            _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
            {
                orderReport.Payment.TotalAmountCharged = @event.Amount;
                orderReport.Payment.TipAmount = @event.Tip;
                orderReport.Payment.MeterAmount = @event.Meter;
                orderReport.Payment.PayPalPayerId = orderReport.Payment.AuthorizationCode = @event.PayPalPayerId;
                orderReport.Payment.PayPalToken = @event.Token;
                orderReport.Payment.TransactionId = @event.TransactionId.ToSafeString().IsNullOrEmpty() ? "" : "Auth: " + @event.TransactionId;
                orderReport.Payment.IsCompleted = true;
            });
        }

        public void Handle(PayPalExpressCheckoutPaymentCancelled @event)
        {
            
            using (var context = _contextFactory.Invoke())
            {
                var payment = context.Find<OrderPaymentDetail>(@event.SourceId);
                if (payment != null)
                {
                    _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
                    {
                        orderReport.Payment.IsCancelled = true;
                    });
                }
            }
        }

        public void Handle(PayPalPaymentCancellationFailed @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var payment = context.Find<OrderPaymentDetail>(@event.SourceId);
                if (payment != null)
                {
                    _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
                    {
                        orderReport.Payment.Error = @event.Reason;
                        orderReport.Payment.IsCancelled = true;
                    });
                }
            }
        }

        public void Handle(OrderCancelled @event)
        {
            _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
            {
                orderReport.OrderStatus.OrderIsCancelled = true;
            });
        }

        public void Handle(OrderCancelledBecauseOfError @event)
        {
            _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
            {
                orderReport.OrderStatus.OrderIsCancelled = true;
            });
        }

        public void Handle(OrderRated @event)
        {
            _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
            {
                var rating = new Dictionary<string, string>();

                foreach (var ratingScore in @event.RatingScores)
                {
                    if (!rating.ContainsKey(ratingScore.Name))
                    {
                        rating.Add(ratingScore.Name, ratingScore.Score.ToString());
                    }
                }

                orderReport.Rating = JsonSerializer.SerializeToString(rating);
            });
        }

        public void Handle(PromotionApplied @event)
        {
            if(!_orderReportProjectionSet.Exists(@event.SourceId))
            {
                return;
            }

            _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
            {
                orderReport.Promotion.WasApplied = true;
                orderReport.Promotion.Code = @event.Code;
            });
        }

        public void Handle(PromotionRedeemed @event)
        {
            _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
            {
                orderReport.Promotion.WasRedeemed = true;
                orderReport.Promotion.SavedAmount = @event.AmountSaved;
            });
        }

        public void Handle(IbsOrderInfoAddedToOrder @event)
        {
            _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
            {
                orderReport.Order.IBSOrderId = @event.IBSOrderId;
            });
        }

        public void Handle(OrderSwitchedToNextDispatchCompany @event)
        {
            _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
            {
                orderReport.Order.CompanyName = @event.CompanyName;
                orderReport.Order.CompanyKey = @event.CompanyKey;
                orderReport.Order.Market = @event.Market;
                orderReport.Order.IBSOrderId = @event.IBSOrderId;
                orderReport.Order.WasSwitchedToAnotherCompany = true;
                orderReport.Order.HasTimedOut = false;
            });
        }

        public void Handle(OrderTimedOut @event)
        {
            _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
            {
                orderReport.Order.HasTimedOut = true;
            });
        }

        public void Handle(PrepaidOrderPaymentInfoUpdated @event)
        {
            _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
            {
                orderReport.Payment.PaymentId = @event.SourceId;
                orderReport.Payment.TotalAmountCharged = @event.Amount;
                orderReport.Payment.MeterAmount = @event.Meter;
                orderReport.Payment.TipAmount = @event.Tip;
                orderReport.Payment.TransactionId = @event.TransactionId;
                orderReport.Payment.Provider = @event.Provider;
                orderReport.Payment.Type = @event.Type;
                orderReport.Payment.IsCompleted = true;
            });
        }

        public void Handle(RefundedOrderUpdated @event)
        {
            _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
            {
                orderReport.Payment.IsRefunded = @event.IsSuccessful;
                orderReport.Payment.Error = @event.Message;
            });
        }

        public void Handle(OrderManuallyPairedForRideLinq @event)
        {
            var account = _accountDetailProjectionSet.GetProjection(@event.AccountId).Load();
            var orderReport = new OrderReportDetail { Id = @event.SourceId };

            orderReport.Account = new OrderReportAccount
            {
                AccountId = @event.AccountId,
                Name = account.Name,
                Phone = account.Settings.Phone,
                Email = account.Email,
                DefaultCardToken = account.DefaultCreditCard,
                PayBack = account.Settings.PayBack
            };

            orderReport.Order = new OrderReportOrder
            {
                ChargeType = ChargeTypes.CardOnFile.Id.ToString(),
                PickupDateTime = @event.PairingDate,
                CreateDateTime = @event.PairingDate,
                PickupAddress = @event.PickupAddress
            };
            orderReport.Client = new OrderReportClient
            {
                OperatingSystem = @event.UserAgent.GetOperatingSystem(),
                UserAgent = @event.UserAgent,
                Version = @event.ClientVersion
            };
                
            orderReport.Payment = new OrderReportPayment
            {
                PairingToken = @event.PairingToken,
                IsPaired = true
            };

            orderReport.OrderStatus = new OrderReportOrderStatus
            {
                Status = OrderStatus.Created
            };

            _orderReportProjectionSet.Add(orderReport);
        }

        public void Handle(OrderUnpairedFromManualRideLinq @event)
        {
            _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
            {
                orderReport.Payment.IsPaired = false;
                orderReport.OrderStatus.OrderIsCancelled = true;
                orderReport.OrderStatus.Status = OrderStatus.Canceled;
            });
        }

        public void Handle(ManualRideLinqTripInfoUpdated @event)
        {
            _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
            {
                orderReport.Payment.MdtFare = @event.Fare;
                orderReport.Payment.MdtTip = @event.Tip;
                orderReport.Payment.MdtToll = @event.Toll;
                orderReport.Payment.Error = @event.PairingError;
                orderReport.Payment.TotalAmountCharged = @event.Total.HasValue
                    ? (decimal?)Math.Round(@event.Total.Value, 2)
                    : null;

                if (@event.EndTime.HasValue)
                {
                    orderReport.OrderStatus.OrderIsCompleted = true;
                    orderReport.OrderStatus.Status = OrderStatus.Completed;
                }
            });
        }

        public void Handle(OriginalEtaLogged @event)
        {
            _orderReportProjectionSet.Update(@event.SourceId, orderReport =>
            {
                orderReport.Order.OriginalEta = @event.OriginalEta;
            });
        }

        private void WrapWithSqlPrimaryKeyHandling(Action eventHandling)
        {
            try
            {
                eventHandling();
            }
            catch (Exception ex)
            {
                var updateException = ex.InnerException as UpdateException;
                var sqlException = updateException != null ? updateException.InnerException as SqlException : null;
                if (sqlException != null && sqlException.Number == SQLPrimaryKeyViolationErrorNumber)
                {
                    // retry
                    _logger.LogMessage("An exception of type SQLPrimaryKeyViolation occurred when inserting in the ReportDetail table. Retrying...");
                    eventHandling();
                }
                else
                {
                    throw;
                }
            }
        }
    }
}