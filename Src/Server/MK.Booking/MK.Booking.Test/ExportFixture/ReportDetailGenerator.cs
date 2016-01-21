#region
using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using NUnit.Framework;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Booking.Test.Integration;
using apcurium.MK.Common;
using ServiceStack.Text;
#endregion

namespace apcurium.MK.Booking.Test.ExportFixture
{
    [TestFixture]
    public class ReportDetailGenerator : given_a_read_model_database
    {
        private const string AccountName = "Antoine Cuvillier";
        private const string OperatingSystem = "Unknown OS";
        private const string TaxiCompany = "My taxi company";
        private const string TaxiCompanyChanged = "My updated taxi company";
        private const string PickupStreet = "Ferrier";
        private const string DropOffStreet = "Mile End";
        private const string VehicleColor = "Blue";
        private const string TransactionIdCcInitiated = "TransactionId On CreditCardPaymentInitiated";
        private const string TransactionIdCcCaptured = "TransactionId On CreditCardPaymentCaptured_V2";
        private const string TransactionIdPaypalCompleted = "TransactionId On PayPalExpressCheckoutPaymentCompleted";
        private const string PromoCode = "promoCode";
        private const int IbsOrderId = 5432;

        private EventHandlers.ReportDetailGenerator _reportDetailGenerator;

        private Guid _accountId;

        private Guid _orderId;

        private Guid _paymentId;

        private Guid _promoId;

        [SetUp]
        public void Setup()
        {
            _paymentId = Guid.NewGuid();
            _orderId = Guid.NewGuid();
            _accountId = Guid.NewGuid();
            _promoId = Guid.NewGuid();

            var accountDetailProjectionSet = new EntityProjectionSet<AccountDetail>(() => new BookingDbContext(DbName));
            var orderReportProjectionSet = new EntityProjectionSet<OrderReportDetail>(() => new BookingDbContext(DbName));
            var orderPaymentProjectionSet = new EntityProjectionSet<OrderPaymentDetail>(() => new BookingDbContext(DbName));
            var creditCardProjectionSet = new EntityProjectionSet<CreditCardDetails>(() => new BookingDbContext(DbName));

            var accountDetailGenerator = new AccountDetailsGenerator(accountDetailProjectionSet);

            accountDetailGenerator.Handle(new AccountRegistered
            {
                SourceId = _accountId,
                Country = CountryCode.GetCountryCodeByIndex(0).CountryISOCode,
                NbPassengers = 1
            });

            accountDetailGenerator.Handle(new AccountConfirmed { SourceId = _accountId });

            _reportDetailGenerator = new EventHandlers.ReportDetailGenerator(
                accountDetailProjectionSet,
                orderReportProjectionSet, 
                orderPaymentProjectionSet,
                creditCardProjectionSet,
                new Logger());
        }

        [Test]
        public void given_a_created_order_and_paired_and_cc_payment_and_rating_and_ibsinfos_and_promotion()
        {
            RaiseOrderCreated();
            RaiseIbsOrderInfoAddedToOrder();
            RaiseOrderStatusChanged(OrderStatus.WaitingForPayment);
            RaisePromotionApplied();
            RaiseOrderPairedForPayment();
            RaiseCreditCardPaymentInitiated();
            RaiseCreditCardPaymentCaptured_V2();
            RaiseOrderStatusChanged(OrderStatus.Completed);
            RaisePromotionRedeemed();
            RaiseOrderRated();

            var orderReportDetail = OrderReportDetail;
            var rating = (JsonSerializer.DeserializeFromString(orderReportDetail.Rating, typeof(Dictionary<string, string>)) as Dictionary<string, string>) ?? new Dictionary<string, string>();

            Assert.AreEqual(orderReportDetail.Account.Name, AccountName);
            Assert.AreEqual(orderReportDetail.Client.OperatingSystem, OperatingSystem);
            Assert.AreEqual(orderReportDetail.Order.CompanyName, TaxiCompanyChanged);
            Assert.AreEqual(orderReportDetail.Order.PickupAddress.Street, PickupStreet);
            Assert.AreEqual(orderReportDetail.Order.DropOffAddress.Street, DropOffStreet);
            Assert.AreEqual(orderReportDetail.Order.IBSOrderId, IbsOrderId);
            Assert.AreEqual(orderReportDetail.OrderStatus.OrderIsCancelled, false);
            Assert.AreEqual(orderReportDetail.OrderStatus.OrderIsCompleted, true);
            Assert.AreEqual(orderReportDetail.VehicleInfos.Color, VehicleColor);
            Assert.AreEqual(orderReportDetail.Payment.IsPaired, true);
            Assert.AreEqual(orderReportDetail.Payment.PreAuthorizedAmount, 25m);
            Assert.AreEqual(orderReportDetail.Payment.TotalAmountCharged, 20m);
            Assert.AreEqual(orderReportDetail.Payment.FirstPreAuthTransactionId, "Auth: " + TransactionIdCcInitiated);
            Assert.AreEqual(orderReportDetail.Payment.TransactionId, "Auth: " + TransactionIdCcCaptured);
            Assert.AreEqual(orderReportDetail.Promotion.WasApplied, true);
            Assert.AreEqual(orderReportDetail.Promotion.Code, PromoCode);
            Assert.AreEqual(orderReportDetail.Promotion.WasRedeemed, true);
            Assert.AreEqual(orderReportDetail.Promotion.SavedAmount, 100);
            Assert.AreEqual(rating["Safety"], "2");
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Dispose();
        }

        private void RaiseOrderCreated()
        {
            _reportDetailGenerator.Handle(new OrderCreated
            {
                SourceId = _orderId,
                AccountId = _accountId,
                PickupAddress = new Address
                {
                    Street = PickupStreet,
                },
                PickupDate = DateTime.Now,
                DropOffAddress = new Address
                {
                    Street = DropOffStreet,
                },
                CreatedDate = DateTime.Now,
                CompanyName = TaxiCompany,
                Settings = new BookingSettings()
                {
                    ChargeType = ChargeTypes.CardOnFile.ToString(),
                    LargeBags = 2,
                    Name = AccountName
                }
            });
        }

        private void RaiseCreditCardPaymentInitiated()
        {
            _reportDetailGenerator.Handle(new CreditCardPaymentInitiated
            {
                SourceId = _paymentId,
                OrderId = _orderId,
                TransactionId = TransactionIdCcInitiated,
                Amount = 25m
            });
        }

        private void RaiseOrderStatusChanged(OrderStatus orderStatus)
        {
            var fakeOrderStatus = new OrderStatusDetail()
            {
                CompanyName = TaxiCompanyChanged,
                Status = orderStatus,
                DriverInfos = new DriverInfos()
                {
                    VehicleColor = VehicleColor,
                }
            };

            _reportDetailGenerator.Handle(new OrderStatusChanged
            {
                Status = fakeOrderStatus,
                SourceId = _orderId,
                IsCompleted = (orderStatus == OrderStatus.Completed),
            });
        }

        private void RaisePayPalExpressCheckoutPaymentInitiated()
        {
            _reportDetailGenerator.Handle(new PayPalExpressCheckoutPaymentInitiated
            {
                SourceId = _paymentId,
                OrderId = _orderId,
                Token = "x"
            });
        }

        private void RaiseCreditCardPaymentCaptured_V2()
        {
            _reportDetailGenerator.Handle(new CreditCardPaymentCaptured_V2
            {
                AuthorizationCode = "x",
                SourceId = _paymentId,
                OrderId = _orderId,
                TransactionId = TransactionIdCcCaptured,
                Amount = 20m
            });
        }

        private void RaisePayPalExpressCheckoutPaymentCompleted()
        {
            _reportDetailGenerator.Handle(new PayPalExpressCheckoutPaymentCompleted
            {
                SourceId = _paymentId,
                OrderId = _orderId,
                Token = "x",
                PayPalPayerId = "x",
                TransactionId = TransactionIdPaypalCompleted,
            });
        }

        private void RaisePromotionApplied()
        {
            _reportDetailGenerator.Handle(new PromotionApplied
            {
                Code = PromoCode,
                OrderId = _orderId,
                SourceId = _promoId,
                AccountId = _accountId,
                DiscountValue = 20,
                DiscountType = PromoDiscountType.Percentage
            });
        }

        private void RaisePromotionRedeemed()
        {
            _reportDetailGenerator.Handle(new PromotionRedeemed
            {
                AmountSaved = 100,
                OrderId = _orderId,
                SourceId = _promoId
            });
        }

        private void RaiseOrderRated()
        {
            _reportDetailGenerator.Handle(new OrderRated
            {
				AccountId = _accountId,
                SourceId = _orderId,
                Note = "x",
                RatingScores = new List<RatingScore>
                {
                    new RatingScore {RatingTypeId = Guid.NewGuid(), Score = 1, Name = "Politness"},
                    new RatingScore {RatingTypeId = Guid.NewGuid(), Score = 2, Name = "Safety"}
                }
            });
        }

        private void RaiseOrderCancelled()
        {
            _reportDetailGenerator.Handle(new OrderCancelled { SourceId = _orderId });
        }

        private void RaiseIbsOrderInfoAddedToOrder()
        {
            _reportDetailGenerator.Handle(new IbsOrderInfoAddedToOrder
            {
                SourceId = _orderId,
                IBSOrderId = IbsOrderId
            });
        }

        private void RaiseOrderPairedForPayment()
        {
            _reportDetailGenerator.Handle(new OrderPairedForPayment { SourceId = _orderId });
        }

        private OrderReportDetail OrderReportDetail
        {
            get
            {
                OrderReportDetail orderReportDetail = null;
                using (var context = new BookingDbContext(DbName))
                {
                    orderReportDetail = context.Find<OrderReportDetail>(_orderId);
                }
                return orderReportDetail;
            }
        }
    }
}