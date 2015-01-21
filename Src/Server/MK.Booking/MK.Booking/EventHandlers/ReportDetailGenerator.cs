using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;
using System.Globalization;

namespace apcurium.MK.Booking.EventHandlers
{
    public class ReportDetailGenerator : IEventHandler<OrderCreated>,
        IEventHandler<OrderStatusChanged>,
        IEventHandler<OrderPairedForPayment>,
        IEventHandler<PayPalExpressCheckoutPaymentCompleted>,
        IEventHandler<OrderCancelled>,
        IEventHandler<PromotionApplied>,
        IEventHandler<PromotionRedeemed>
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
                    AccountId = @event.AccountId,
                    Name = @event.Settings.Name,
                    Phone = @event.Settings.Phone,
                    Email = accountDetails.Email,
                    IBSAccountId = accountDetails.IBSAccountId,
                    OrderId = @event.SourceId,
                    IBSOrderId = @event.IBSOrderId,
                    ChargeType = @event.Settings.ChargeType,
                    PickupDateTime = @event.PickupDate,
                    CreateDateTime = @event.CreatedDate,
                    PickupAddress = @event.PickupAddress,
                    DropOffAddress = @event.DropOffAddress,
                    VehicleCompanyName = @event.CompanyName,
                    ClientOperatingSystem = @event.UserAgent.GetOperatingSystem(),
                    ClientUserAgent = @event.UserAgent,
                    ClientVersion = @event.ClientVersion,
                    DefaultCardToken = accountDetails.DefaultCreditCard
                });
            }
        }

        public void Handle(OrderStatusChanged @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = GetOrderReportDetail(context, @event.SourceId);
                var orderPayment = context.Find<OrderPaymentDetail>(@event.SourceId);
                orderReport.PaymentMeterAmount = orderPayment.Meter;
                orderReport.PaymentTipAmount = orderPayment.Tip;
                orderReport.PaymentTotalAmountCharged = orderPayment.Amount;
                orderReport.PaymentProvider = orderPayment.Provider;
                orderReport.PaymentType = orderPayment.Type;
                orderReport.PaymentTransactionId = orderPayment.TransactionId;
                orderReport.PaymentCardToken = orderPayment.CardToken;
                orderReport.OrderIsCompleted = @event.IsCompleted;
                orderReport.MdtFare = @event.Fare;
                orderReport.MdtTip = @event.Tip;
                orderReport.MdtToll = @event.Toll;
                orderReport.AccountId = @event.Status.AccountId;
                orderReport.VehicleCompanyName = @event.Status.CompanyName;
                orderReport.VehicleDriverFirstName = @event.Status.DriverInfos.FirstName;
                orderReport.VehicleDriverLastName = @event.Status.DriverInfos.LastName;
                orderReport.VehicleColor = @event.Status.DriverInfos.VehicleColor;
                orderReport.VehicleMake = @event.Status.DriverInfos.VehicleMake;
                orderReport.VehicleModel = @event.Status.DriverInfos.VehicleModel;
                orderReport.VehicleRegistration = @event.Status.DriverInfos.VehicleRegistration;
                orderReport.VehicleType = @event.Status.DriverInfos.VehicleType;
                orderReport.OrderStatus = (int) @event.Status.Status;
                orderReport.PickupDateTime = @event.Status.PickupDate;

                context.Save(orderReport);
            }
        }

        public void Handle(OrderPairedForPayment @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = GetOrderReportDetail(context, @event.SourceId);
                orderReport.VehicleWasConfirmed = true;
                context.Save(orderReport);
            }
        }

        public void Handle(PayPalExpressCheckoutPaymentCompleted @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = GetOrderReportDetail(context, @event.OrderId);
                orderReport.PayPalPayerId = @event.PayPalPayerId;
                orderReport.PayPalToken = @event.Token;
                context.Save(orderReport);
            }
        }

        public void Handle(OrderCancelled @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = GetOrderReportDetail(context, @event.SourceId);
                orderReport.OrderIsCancelled = true;
                context.Save(orderReport);
            }
        }

        public void Handle(PromotionApplied @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = GetOrderReportDetail(context, @event.OrderId);
                orderReport.PromotionApplied = true;
                orderReport.PromotionCode = @event.Code;
                context.Save(orderReport);
            }
        }

        public void Handle(PromotionRedeemed @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderReport = GetOrderReportDetail(context, @event.OrderId);
                orderReport.PromotionRedeemed = true;
                orderReport.PromotionSavedAmount = @event.AmountSaved;
                context.Save(orderReport);
            }
        }

        private OrderReportDetail GetOrderReportDetail(BookingDbContext context, Guid orderId)
        {
            return context.Find<OrderReportDetail>(orderId);
        }
    }
}