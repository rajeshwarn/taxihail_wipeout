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
        IEventHandler<CreditCardPaymentInitiated>,
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
                
                context.Save(new OrderReportDetail
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
                    OrderStatus = new OrderReportOrderStatus(),
                    Payment = new OrderReportPayment(),
                    Promotion = new OrderReportPromotion(),
                    VehicleInfos = new OrderReportVehicleInfos(),
                    Rating = ""
                });
            }
        }

        public void Handle(OrderStatusChanged @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = context.Find<OrderReportDetail>(@event.SourceId);
                
                var account = orderReport.Account ?? new OrderReportAccount();
                var order = orderReport.Order ?? new OrderReportOrder();
                var orderStatus = orderReport.OrderStatus ?? new OrderReportOrderStatus();
                var payment = orderReport.Payment ?? new OrderReportPayment();
                var vehicleInfos = orderReport.VehicleInfos ?? new OrderReportVehicleInfos();
                
                account.AccountId = @event.Status.AccountId;

                payment.MdtFare = @event.Fare;
                payment.MdtTip = @event.Tip;
                payment.MdtToll = @event.Toll;
                
                vehicleInfos.DriverFirstName = @event.Status.DriverInfos.FirstName;
                vehicleInfos.DriverLastName = @event.Status.DriverInfos.LastName;
                vehicleInfos.Number = @event.Status.VehicleNumber;
                vehicleInfos.Color = @event.Status.DriverInfos.VehicleColor;
                vehicleInfos.Make = @event.Status.DriverInfos.VehicleMake;
                vehicleInfos.Model = @event.Status.DriverInfos.VehicleModel;
                vehicleInfos.Registration = @event.Status.DriverInfos.VehicleRegistration;
                vehicleInfos.Type = @event.Status.DriverInfos.VehicleType;
                
                orderStatus.Status = (int)@event.Status.Status;
                orderStatus.OrderIsCompleted = @event.IsCompleted;
                
                order.PickupDateTime = @event.Status.PickupDate;
                order.CompanyName = @event.Status.CompanyName;
                
                orderReport.Account = account;
                orderReport.Order = order;
                orderReport.OrderStatus = orderStatus;
                orderReport.Payment = payment;
                orderReport.VehicleInfos = vehicleInfos;

                context.Save(orderReport);
            }
        }

        public void Handle(OrderPairedForPayment @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = context.Find<OrderReportDetail>(@event.SourceId);
                var vehicleInfos = orderReport.VehicleInfos ?? new OrderReportVehicleInfos();
                vehicleInfos.WasConfirmed = true;
                orderReport.VehicleInfos = vehicleInfos;
                context.Save(orderReport);
            }
        }

        public void Handle(CreditCardPaymentInitiated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = context.Find<OrderReportDetail>(@event.OrderId);
                var payment = orderReport.Payment ?? new OrderReportPayment();
                payment.CardToken = @event.CardToken;
                payment.Type = PaymentType.CreditCard;
                orderReport.Payment = payment;
                context.Save(orderReport);
            }
        }

        public void Handle(PayPalExpressCheckoutPaymentInitiated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = context.Find<OrderReportDetail>(@event.OrderId);
                var payment = orderReport.Payment ?? new OrderReportPayment();
                payment.PayPalToken = @event.Token;
                payment.Type = PaymentType.PayPal;
                orderReport.Payment = payment;
                context.Save(orderReport);
            }
        }

        public void Handle(PayPalExpressCheckoutPaymentCompleted @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = context.Find<OrderReportDetail>(@event.OrderId);
                var payment = orderReport.Payment ?? new OrderReportPayment();
                payment.PalPayerId = @event.PayPalPayerId;
                payment.PayPalToken = @event.Token;
                orderReport.Payment = payment;
                context.Save(orderReport);
            }
        }

        public void Handle(OrderCancelled @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = context.Find<OrderReportDetail>(@event.SourceId);
                var status = orderReport.OrderStatus ?? new OrderReportOrderStatus();
                status.OrderIsCancelled = true;
                orderReport.OrderStatus = status;
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
                var promo = orderReport.Promotion ?? new OrderReportPromotion();
                promo.WasApplied = true;
                promo.Code = @event.Code;
                orderReport.Promotion = promo;
                context.Save(orderReport);
            }
        }

        public void Handle(PromotionRedeemed @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = context.Find<OrderReportDetail>(@event.OrderId);
                var promo = orderReport.Promotion ?? new OrderReportPromotion();
                promo.WasRedeemed = true;
                promo.SavedAmount = @event.AmountSaved;
                orderReport.Promotion = promo;
                context.Save(orderReport);
            }
        }

        public void Handle(CreditCardPaymentCaptured_V2 @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = context.Find<OrderReportDetail>(@event.OrderId);

                orderReport.Payment = new OrderReportPayment()
                {
                    AuthorizationCode = @event.AuthorizationCode,
                    MeterAmount = @event.Meter,
                    TipAmount = @event.Tip,
                    TotalAmountCharged = @event.Amount,
                    Provider = @event.Provider,
                    TransactionId = @event.TransactionId.ToSafeString().IsNullOrEmpty() ? "" : "Auth: " + @event.TransactionId
                };

                context.Save(orderReport);
            }
        }

        public void Handle(IbsOrderInfoAddedToOrder @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = context.Find<OrderReportDetail>(@event.SourceId);
                var order = orderReport.Order ?? new OrderReportOrder();
                order.IBSOrderId = @event.IBSOrderId;
                orderReport.Order = order;
                context.Save(orderReport);
            }
        }
    }
}