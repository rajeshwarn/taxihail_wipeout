using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;
using System.Collections.Generic;
using ServiceStack.Text;


namespace apcurium.MK.Booking.EventHandlers
{
    public class ReportDetailGenerator : IEventHandler<OrderCreated>,
        IEventHandler<OrderStatusChanged>,
        IEventHandler<OrderPairedForPayment>,
        IEventHandler<PayPalExpressCheckoutPaymentInitiated>,
        IEventHandler<PayPalExpressCheckoutPaymentCompleted>,
        IEventHandler<OrderCancelled>,
        IEventHandler<OrderRated>,
        IEventHandler<PromotionApplied>,
        IEventHandler<PromotionRedeemed>,
        IEventHandler<CreditCardPaymentCaptured_V2>,
        IEventHandler<IbsOrderInfoAddedToOrder>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly ILogger _logger;
        private readonly IServerSettings _serverSettings;
        private readonly Resources.Resources _resources;
        public ReportDetailGenerator(Func<BookingDbContext> contextFactory,
            ILogger logger,
            IServerSettings serverSettings)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            _serverSettings = serverSettings;
            _resources = new Resources.Resources(serverSettings);
        }

        public void Handle(OrderCreated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var accountDetails = context.Query<AccountDetail>().FirstOrDefault(x => x.Id == @event.AccountId);

                context.Save(new OrderReportDetail()
                {
                    Id = @event.SourceId,
                    Account = new OrderReportAccount()
                    {
                        AccountId = @event.AccountId,
                        Name = @event.Settings.Name,
                        Phone = @event.Settings.Phone,
                        Email = accountDetails.Email,
                        DefaultCardToken = accountDetails.DefaultCreditCard,
                        IBSAccountId = accountDetails.IBSAccountId
                    },
                    Order = new OrderReportOrder()
                    {
                        IBSOrderId = @event.IBSOrderId,
                        ChargeType = @event.Settings.ChargeType,
                        PickupDateTime = @event.PickupDate,
                        CreateDateTime = @event.CreatedDate,
                        PickupAddress = @event.PickupAddress,
                        DropOffAddress = @event.DropOffAddress,
                        CompanyName = @event.CompanyName
                    },
                    Client = new OrderReportClient()
                    {
                        OperatingSystem = @event.UserAgent.GetOperatingSystem(),
                        UserAgent = @event.UserAgent,
                        Version = @event.ClientVersion
                    },
                    Rating = ""
                });
            }
        }

        public void Handle(OrderStatusChanged @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = context.Find<OrderReportDetail>(@event.SourceId);

                orderReport.Account.AccountId = @event.Status.AccountId;

                orderReport.Payment.MdtFare = @event.Fare;
                orderReport.Payment.MdtTip = @event.Tip;
                orderReport.Payment.MdtToll = @event.Toll;
                
                var orderPaymentDetail = context.Set<OrderPaymentDetail>().FirstOrDefault(payment => payment.OrderId == @event.SourceId);
                
                if (orderPaymentDetail != null)
                {
                    orderReport.Payment.MeterAmount = orderPaymentDetail.Meter;
                    orderReport.Payment.TipAmount = orderPaymentDetail.Tip;
                    orderReport.Payment.TotalAmountCharged = orderPaymentDetail.Amount;
                    orderReport.Payment.Type = orderPaymentDetail.Type;
                    orderReport.Payment.Provider = orderPaymentDetail.Provider;
                    orderReport.Payment.CardToken = orderPaymentDetail.CardToken;
                    orderReport.Payment.TransactionId = orderPaymentDetail.TransactionId.ToSafeString().IsNullOrEmpty() ? "" : "Auth: " + orderPaymentDetail.TransactionId;
                    orderReport.Payment.AuthorizationCode = orderPaymentDetail.AuthorizationCode;
                }

                orderReport.VehicleInfos.DriverFirstName = @event.Status.DriverInfos.FirstName;
                orderReport.VehicleInfos.DriverLastName = @event.Status.DriverInfos.LastName;
                orderReport.VehicleInfos.Number = @event.Status.VehicleNumber;
                orderReport.VehicleInfos.Color = @event.Status.DriverInfos.VehicleColor;
                orderReport.VehicleInfos.Make = @event.Status.DriverInfos.VehicleMake;
                orderReport.VehicleInfos.Model = @event.Status.DriverInfos.VehicleModel;
                orderReport.VehicleInfos.Registration = @event.Status.DriverInfos.VehicleRegistration;
                orderReport.VehicleInfos.Type = @event.Status.DriverInfos.VehicleType;

                orderReport.OrderStatus.Status = (int)@event.Status.Status;
                orderReport.OrderStatus.OrderIsCompleted = @event.IsCompleted;

                orderReport.Order.PickupDateTime = @event.Status.PickupDate;
                orderReport.Order.CompanyName = @event.Status.CompanyName;

                context.Save(orderReport);
            }
        }

        public void Handle(OrderPairedForPayment @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = context.Find<OrderReportDetail>(@event.SourceId);
                orderReport.VehicleInfos.WasConfirmed = true;
                context.Save(orderReport);
            }
        }

        public void Handle(CreditCardPaymentInitiated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = context.Find<OrderReportDetail>(@event.OrderId);
                if (orderReport != null)
                {
                    orderReport.Payment.CardToken = @event.CardToken;
                    orderReport.Payment.Type = PaymentType.CreditCard;
                    context.Save(orderReport);
                }
                
            }
        }

        public void Handle(PayPalExpressCheckoutPaymentInitiated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = context.Find<OrderReportDetail>(@event.OrderId);
                if (orderReport != null)
                {
                    orderReport.Payment.PayPalToken = @event.Token;
                    orderReport.Payment.TotalAmountCharged = @event.Amount;
                    orderReport.Payment.TipAmount = @event.Tip;
                    orderReport.Payment.MeterAmount = @event.Meter;
                    orderReport.Payment.Provider = PaymentProvider.PayPal;
                    orderReport.Payment.Type = PaymentType.PayPal;
                    context.Save(orderReport);
                }
                
            }
        }

        public void Handle(PayPalExpressCheckoutPaymentCompleted @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = context.Find<OrderReportDetail>(@event.OrderId);
                orderReport.Payment.TotalAmountCharged = @event.Amount;
                orderReport.Payment.TipAmount = @event.Tip;
                orderReport.Payment.MeterAmount = @event.Meter;
                orderReport.Payment.Provider = PaymentProvider.PayPal;
                orderReport.Payment.Type = PaymentType.PayPal;
                orderReport.Payment.PalPayerId = orderReport.Payment.AuthorizationCode = @event.PayPalPayerId;
                orderReport.Payment.PayPalToken = @event.Token;
                orderReport.Payment.TransactionId = @event.TransactionId.ToSafeString().IsNullOrEmpty() ? "" : "Auth: " + @event.TransactionId;
                context.Save(orderReport);
            }
        }

        public void Handle(OrderCancelled @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = context.Find<OrderReportDetail>(@event.SourceId);
                orderReport.OrderStatus.OrderIsCancelled = true;
                context.Save(orderReport);
            }
        }

        public void Handle(OrderRated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = context.Find<OrderReportDetail>(@event.SourceId);

                var rating = new Dictionary<string, string>();

                foreach (var ratingScore in @event.RatingScores)
                {
                    rating.Add(ratingScore.Name, ratingScore.Score.ToString());
                }

                orderReport.Rating = JsonSerializer.SerializeToString(rating);

                context.SaveChanges();
            }
        }

        public void Handle(PromotionApplied @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = context.Find<OrderReportDetail>(@event.OrderId);

                if (orderReport != null)
                {
                    orderReport.Promotion.WasApplied = true;
                    orderReport.Promotion.Code = @event.Code;
                    context.Save(orderReport);
                }
            }
        }

        public void Handle(PromotionRedeemed @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = context.Find<OrderReportDetail>(@event.OrderId);
                orderReport.Promotion.WasRedeemed = true;
                orderReport.Promotion.SavedAmount = @event.AmountSaved;
                context.Save(orderReport);
            }
        }

        public void Handle(CreditCardPaymentCaptured_V2 @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = context.Find<OrderReportDetail>(@event.OrderId);
                orderReport.Payment.Type = PaymentType.CreditCard;
                orderReport.Payment.AuthorizationCode = @event.AuthorizationCode;
                orderReport.Payment.MeterAmount = @event.Meter;
                orderReport.Payment.TipAmount = @event.Tip;
                orderReport.Payment.TotalAmountCharged = @event.Amount;
                orderReport.Payment.Provider = @event.Provider;
                orderReport.Payment.TransactionId = @event.TransactionId.ToSafeString().IsNullOrEmpty() ? "" : "Auth: " + @event.TransactionId;
                context.Save(orderReport);
            }
        }

        public void Handle(IbsOrderInfoAddedToOrder @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = context.Find<OrderReportDetail>(@event.SourceId);
                orderReport.Order.IBSOrderId = @event.IBSOrderId;
                context.Save(orderReport);
            }
        }
    }
}