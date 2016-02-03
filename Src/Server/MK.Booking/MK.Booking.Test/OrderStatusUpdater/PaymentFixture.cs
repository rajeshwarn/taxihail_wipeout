using System;
using System.Linq;
using System.Threading;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.OrderStatusUpdater
{
    [TestFixture]
    public class PaymentFixture : given_a_read_model_database_for_orderstatusupdater
    {
        [Test]
        public void when_order_is_paired_and_received_fare_with_preauth_disabled()
        {
            // Prepare
            var accountId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            string companyKey = null;
            var orderAmount = 100m;

            var creditCardId = Guid.NewGuid();

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new AccountDetail
                {
                    Id = accountId,
                    CreationDate = DateTime.Now,
                    IBSAccountId = 123,
                    DefaultCreditCard = creditCardId
                });

                context.Save(new CreditCardDetails
                {
                    AccountId = accountId,
                    CreditCardId = creditCardId,
                    Token = "token"
                });

                context.Save(new OrderDetail
                {
                    Id = orderId,
                    AccountId = accountId,
                    PickupDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    IBSOrderId = 12345,
                    CompanyKey = companyKey
                });

                context.Save(new OrderPairingDetail
                {
                    OrderId = orderId
                });
            }
            
            var tip = FareHelper.CalculateTipAmount(orderAmount, ConfigurationManager.ServerData.DefaultTipPercentage);

            var ibsOrder = new IBSOrderInformation { Fare = Convert.ToDouble(orderAmount) };
            var status = new OrderStatusDetail { OrderId = orderId, CompanyKey = companyKey, AccountId = accountId, IBSOrderId = 12345 };

            ConfigurationManager.SetPaymentSettings(null, new ServerPaymentSettings { PaymentMode = PaymentMethod.Braintree });

            EnsurePreAuthPaymentForTripWasCalled(status, orderAmount + tip);
            EnsureCommitWasCalled(status, orderAmount + tip, orderAmount + tip, orderAmount, tip);

            // Act
            Sut.Update(ibsOrder, status);

            // Assert
            PaymentServiceMock.Verify();

            Assert.AreEqual(2, Commands.Count);
            Assert.AreEqual(typeof(CaptureCreditCardPayment), Commands.First().GetType());
            Assert.AreEqual(typeof(ChangeOrderStatus), Commands.Skip(1).First().GetType());
        }

        [Test]
        public void when_order_is_paired_and_received_fare_with_preauth_disabled_and_preauth_fails()
        {
            // Prepare
            var accountId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            string companyKey = null;
            var orderAmount = 100.55m;

            var creditCardId = Guid.NewGuid();

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new AccountDetail
                {
                    Id = accountId,
                    CreationDate = DateTime.Now,
                    IBSAccountId = 123,
                    DefaultCreditCard = creditCardId
                });

                context.Save(new CreditCardDetails
                {
                    AccountId = accountId,
                    CreditCardId = creditCardId,
                    Token = "token"
                });

                context.Save(new OrderDetail
                {
                    Id = orderId,
                    AccountId = accountId,
                    PickupDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    IBSOrderId = 12345,
                    CompanyKey = companyKey
                });

                context.Save(new OrderPairingDetail
                {
                    OrderId = orderId
                });
            }

            var tip = FareHelper.CalculateTipAmount(orderAmount, ConfigurationManager.ServerData.DefaultTipPercentage);

            var ibsOrder = new IBSOrderInformation { Fare = Convert.ToDouble(orderAmount) };
            var status = new OrderStatusDetail { OrderId = orderId, CompanyKey = companyKey, AccountId = accountId, IBSOrderId = 12345 };

            ConfigurationManager.SetPaymentSettings(null, new ServerPaymentSettings { PaymentMode = PaymentMethod.Braintree });

            EnsurePreAuthPaymentForTripWasCalled(status, orderAmount + tip);

            // Act
            Sut.Update(ibsOrder, status);

            // Assert
            PaymentServiceMock.Verify();

            Assert.AreEqual(1, Commands.Count);
            Assert.AreEqual(typeof(ChangeOrderStatus), Commands.First().GetType());
        }

        [Test]
        public void when_order_is_paired_and_received_fare_with_preauth_disabled_and_preauth_fails_with_card_declined()
        {
            // Prepare
            var accountId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            string companyKey = null;
            var orderAmount = 100.85m;

            var creditCardId = Guid.NewGuid();

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new AccountDetail
                {
                    Id = accountId,
                    CreationDate = DateTime.Now,
                    IBSAccountId = 123,
                    DefaultCreditCard = creditCardId
                });

                context.Save(new CreditCardDetails
                {
                    AccountId = accountId,
                    CreditCardId = creditCardId,
                    Token = "token"
                });

                context.Save(new OrderDetail
                {
                    Id = orderId,
                    AccountId = accountId,
                    PickupDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    IBSOrderId = 12345,
                    CompanyKey = companyKey
                });

                context.Save(new OrderPairingDetail
                {
                    OrderId = orderId
                });
            }

            var tip = FareHelper.CalculateTipAmount(orderAmount, ConfigurationManager.ServerData.DefaultTipPercentage);

            var ibsOrder = new IBSOrderInformation { Fare = Convert.ToDouble(orderAmount) };
            var status = new OrderStatusDetail { OrderId = orderId, CompanyKey = companyKey, AccountId = accountId, IBSOrderId = 12345 };

            ConfigurationManager.SetPaymentSettings(null, new ServerPaymentSettings { PaymentMode = PaymentMethod.Braintree });

            EnsurePreAuthPaymentForTripWasCalled(status, orderAmount + tip);

            // Act
            Sut.Update(ibsOrder, status);

            // Assert
            PaymentServiceMock.Verify();

            Assert.AreEqual(2, Commands.Count);
            Assert.AreEqual(typeof(ReactToPaymentFailure), Commands.First().GetType());
            Assert.AreEqual(typeof(ChangeOrderStatus), Commands.Skip(1).First().GetType());
        }

        [Test]
        public void when_order_is_paired_and_received_fare_with_preauth_enabled()
        {
            // Prepare
            var accountId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            string companyKey = null;
            var orderAmount = 100m;
            var preAuthAmount = 50m;

            var creditCardId = Guid.NewGuid();

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new AccountDetail
                {
                    Id = accountId,
                    CreationDate = DateTime.Now,
                    IBSAccountId = 123,
                    DefaultCreditCard = creditCardId
                });

                context.Save(new CreditCardDetails
                {
                    AccountId = accountId,
                    CreditCardId = creditCardId,
                    Token = "token"
                });

                context.Save(new OrderDetail
                {
                    Id = orderId,
                    AccountId = accountId,
                    PickupDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    IBSOrderId = 12345,
                    CompanyKey = companyKey
                });

                context.Save(new OrderPaymentDetail
                {
                    PaymentId = Guid.NewGuid(),
                    OrderId = orderId,
                    CompanyKey = companyKey,
                    PreAuthorizedAmount = preAuthAmount,
                    FirstPreAuthTransactionId = "asdasdnasd",
                    TransactionId = "asdasdnasd"
                });

                context.Save(new OrderPairingDetail
                {
                    OrderId = orderId
                });
            }

            var tip = FareHelper.CalculateTipAmount(orderAmount, ConfigurationManager.ServerData.DefaultTipPercentage);

            var ibsOrder = new IBSOrderInformation { Fare = Convert.ToDouble(orderAmount) };
            var status = new OrderStatusDetail { OrderId = orderId, CompanyKey = companyKey, AccountId = accountId, IBSOrderId = 12345 };

            ConfigurationManager.SetPaymentSettings(null, new ServerPaymentSettings { PaymentMode = PaymentMethod.Braintree });

            EnsureCommitWasCalled(status, preAuthAmount, orderAmount + tip, orderAmount, tip);

            // Act
            Sut.Update(ibsOrder, status);

            // Assert
            PaymentServiceMock.Verify();

            Assert.AreEqual(2, Commands.Count);
            Assert.AreEqual(typeof(CaptureCreditCardPayment), Commands.First().GetType());
            Assert.AreEqual(typeof(ChangeOrderStatus), Commands.Skip(1).First().GetType());
        }

        [Test]
        public void when_order_is_paired_and_received_fare_with_preauth_enabled_and_commit_fails()
        {
            // Prepare
            var accountId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var creditCardId = Guid.NewGuid();
            string companyKey = null;
            var orderAmount = 100.55m;
            var preAuthAmount = 50m;

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new AccountDetail
                {
                    Id = accountId,
                    CreationDate = DateTime.Now,
                    IBSAccountId = 123,
                    DefaultCreditCard = creditCardId,
                });

                context.Save(new CreditCardDetails
                {
                    AccountId = accountId,
                    CreditCardId = creditCardId,
                    Token = "token"
                });

                context.Save(new OrderDetail
                {
                    Id = orderId,
                    AccountId = accountId,
                    PickupDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    IBSOrderId = 12345,
                    CompanyKey = companyKey
                });

                context.Save(new OrderPaymentDetail
                {
                    PaymentId = Guid.NewGuid(),
                    OrderId = orderId,
                    CompanyKey = companyKey,
                    PreAuthorizedAmount = preAuthAmount,
                    FirstPreAuthTransactionId = "asdasdnasd",
                    TransactionId = "asdasdnasd"
                });

                context.Save(new OrderPairingDetail
                {
                    OrderId = orderId
                });
            }

            var tip = FareHelper.CalculateTipAmount(orderAmount, ConfigurationManager.ServerData.DefaultTipPercentage);

            var ibsOrder = new IBSOrderInformation { Fare = Convert.ToDouble(orderAmount) };
            var status = new OrderStatusDetail { OrderId = orderId, CompanyKey = companyKey, AccountId = accountId, IBSOrderId = 12345 };

            ConfigurationManager.SetPaymentSettings(null, new ServerPaymentSettings { PaymentMode = PaymentMethod.Braintree });

            EnsureCommitWasCalled(status, preAuthAmount, orderAmount + tip, orderAmount, tip);
            EnsureVoidPreAuthWasCalled(status);

            // Act
            Sut.Update(ibsOrder, status);

            // Assert
            PaymentServiceMock.Verify();

            Assert.AreEqual(2, Commands.Count);
            Assert.AreEqual(typeof(LogCreditCardError), Commands.First().GetType());
            Assert.AreEqual(typeof(ChangeOrderStatus), Commands.Skip(1).First().GetType());
        }

        [Test]
        public void when_order_is_paired_and_received_fare_with_preauth_enabled_and_commit_fails_with_card_declined()
        {
            // Prepare
            var accountId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var creditCardId = Guid.NewGuid();
            string companyKey = null;
            var orderAmount = 100.85m;
            var preAuthAmount = 50m;

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new AccountDetail
                {
                    Id = accountId,
                    CreationDate = DateTime.Now,
                    IBSAccountId = 123,
                    DefaultCreditCard = creditCardId
                });

                context.Save(new CreditCardDetails
                {
                    AccountId = accountId,
                    CreditCardId = creditCardId,
                    Token = "token"
                });

                context.Save(new OrderDetail
                {
                    Id = orderId,
                    AccountId = accountId,
                    PickupDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    IBSOrderId = 12345,
                    CompanyKey = companyKey
                });

                context.Save(new OrderPaymentDetail
                {
                    PaymentId = Guid.NewGuid(),
                    OrderId = orderId,
                    CompanyKey = companyKey,
                    PreAuthorizedAmount = preAuthAmount,
                    FirstPreAuthTransactionId = "asdasdnasd",
                    TransactionId = "asdasdnasd"
                });

                context.Save(new OrderPairingDetail
                {
                    OrderId = orderId
                });
            }

            var tip = FareHelper.CalculateTipAmount(orderAmount, ConfigurationManager.ServerData.DefaultTipPercentage);

            var ibsOrder = new IBSOrderInformation { Fare = Convert.ToDouble(orderAmount) };
            var status = new OrderStatusDetail { OrderId = orderId, CompanyKey = companyKey, AccountId = accountId, IBSOrderId = 12345 };

            ConfigurationManager.SetPaymentSettings(null, new ServerPaymentSettings { PaymentMode = PaymentMethod.Braintree });

            EnsureCommitWasCalled(status, preAuthAmount, orderAmount + tip, orderAmount, tip);
            EnsureVoidPreAuthWasCalled(status);

            // Act
            Sut.Update(ibsOrder, status);

            // Assert
            PaymentServiceMock.Verify();

            Assert.AreEqual(3, Commands.Count);
            Assert.AreEqual(typeof(LogCreditCardError), Commands.First().GetType());
            Assert.AreEqual(typeof(ReactToPaymentFailure), Commands.Skip(1).First().GetType());
            Assert.AreEqual(typeof(ChangeOrderStatus), Commands.Skip(2).First().GetType());
        }

        [Test]
        public void when_order_on_local_company_has_booking_fees_with_preauth_disabled()
        {
            // Prepare
            var accountId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var creditCardId = Guid.NewGuid();
            string companyKey = null;
            var orderAmount = 100m;
            var bookingFees = 40m;

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new AccountDetail
                {
                    Id = accountId,
                    CreationDate = DateTime.Now,
                    IBSAccountId = 123,
                    DefaultCreditCard = creditCardId
                });

                context.Save(new CreditCardDetails
                {
                    AccountId = accountId,
                    CreditCardId = creditCardId,
                    Token = "token"
                });

                context.Save(new OrderDetail
                {
                    Id = orderId,
                    AccountId = accountId,
                    PickupDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    IBSOrderId = 12345,
                    CompanyKey = companyKey,
                    BookingFees = bookingFees
                });

                context.Save(new OrderPairingDetail
                {
                    OrderId = orderId
                });
            }

            var tip = FareHelper.CalculateTipAmount(orderAmount, ConfigurationManager.ServerData.DefaultTipPercentage);

            var ibsOrder = new IBSOrderInformation { Fare = Convert.ToDouble(orderAmount) };
            var status = new OrderStatusDetail { OrderId = orderId, CompanyKey = companyKey, AccountId = accountId, IBSOrderId = 12345 };

            ConfigurationManager.SetPaymentSettings(null, new ServerPaymentSettings { PaymentMode = PaymentMethod.Braintree });

            EnsurePreAuthPaymentForTripWasCalled(status, orderAmount + tip + bookingFees);
            EnsureCommitWasCalled(status, orderAmount + tip + bookingFees, orderAmount + tip + bookingFees, orderAmount, tip);

            // Act
            Sut.Update(ibsOrder, status);

            // Assert
            PaymentServiceMock.Verify();

            Assert.AreEqual(2, Commands.Count);
            Assert.AreEqual(typeof(CaptureCreditCardPayment), Commands.First().GetType());
            Assert.AreEqual(typeof(ChangeOrderStatus), Commands.Skip(1).First().GetType());
        }

        [Test]
        public void when_order_on_external_company_has_booking_fees_but_configured_with_other_than_cmt_or_ridelinq()
        {
            // Prepare
            var accountId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var creditCardId = Guid.NewGuid();
            string companyKey = "ext";
            var orderAmount = 100m;
            var bookingFees = 40m;

            using (var context = new BookingDbContext(DbName))
            {
                context.RemoveAll<FeesDetail>();
                context.SaveChanges();

                context.Save(new AccountDetail
                {
                    Id = accountId,
                    CreationDate = DateTime.Now,
                    IBSAccountId = 123,
                    DefaultCreditCard = creditCardId
                });

                context.Save(new AccountIbsDetail
                {
                    CompanyKey = companyKey,
                    AccountId = accountId,
                    IBSAccountId = 123,
                });

                context.Save(new CreditCardDetails
                {
                    AccountId = accountId,
                    CreditCardId = creditCardId,
                    Token = "token"
                });

                context.Save(new OrderDetail
                {
                    Id = orderId,
                    AccountId = accountId,
                    PickupDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    IBSOrderId = 12345,
                    CompanyKey = companyKey,
                    BookingFees = bookingFees
                });

                context.Save(new OrderPairingDetail
                {
                    OrderId = orderId
                });

                context.Save(new FeesDetail
                {
                    Market = null,
                    Booking = bookingFees,
                    Id = Guid.NewGuid()
                });
            }

            var tip = FareHelper.CalculateTipAmount(orderAmount, ConfigurationManager.ServerData.DefaultTipPercentage);

            var ibsOrder = new IBSOrderInformation { Fare = Convert.ToDouble(orderAmount) };
            var status = new OrderStatusDetail { OrderId = orderId, CompanyKey = companyKey, AccountId = accountId, IBSOrderId = 12345 };

            ConfigurationManager.SetPaymentSettings(null, new ServerPaymentSettings { PaymentMode = PaymentMethod.Braintree });
            ConfigurationManager.SetPaymentSettings(companyKey, new ServerPaymentSettings { PaymentMode = PaymentMethod.Braintree });

            EnsurePreAuthPaymentForTripWasCalled(status, orderAmount + tip);
            EnsureCommitWasCalled(status, orderAmount + tip, orderAmount + tip, orderAmount, tip);

            // Act
            Sut.Update(ibsOrder, status);

            // Assert
            PaymentServiceMock.Verify();

            Assert.AreEqual(2, Commands.Count);
            Assert.AreEqual(typeof(CaptureCreditCardPayment), Commands.First().GetType());
            Assert.AreEqual(typeof(ChangeOrderStatus), Commands.Skip(1).First().GetType());
        }

        [Test]
        public void when_order_on_external_company_has_booking_fees_configured_with_cmt()
        {
            // Prepare
            var creditCardId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            string companyKey = "ext";
            var orderAmount = 100m;
            var bookingFees = 40m;

            using (var context = new BookingDbContext(DbName))
            {
                context.RemoveAll<FeesDetail>();
                context.SaveChanges();

                context.Save(new AccountDetail
                {
                    Id = accountId,
                    CreationDate = DateTime.Now,
                    IBSAccountId = 123,
                    DefaultCreditCard = creditCardId
                });

                context.Save(new AccountIbsDetail
                {
                    CompanyKey = companyKey,
                    AccountId = accountId,
                    IBSAccountId = 123
                });

                context.Save(new CreditCardDetails
                {
                    AccountId = accountId,
                    CreditCardId = creditCardId,
                    Token = "token"
                });

                context.Save(new OrderDetail
                {
                    Id = orderId,
                    AccountId = accountId,
                    PickupDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    IBSOrderId = 12345,
                    CompanyKey = companyKey,
                    BookingFees = bookingFees
                });

                context.Save(new OrderPairingDetail
                {
                    OrderId = orderId
                });

                context.Save(new FeesDetail
                {
                    Market = null,
                    Booking = bookingFees,
                    Id = Guid.NewGuid()
                });
            }

            var tip = FareHelper.CalculateTipAmount(orderAmount, ConfigurationManager.ServerData.DefaultTipPercentage);

            var ibsOrder = new IBSOrderInformation { Fare = Convert.ToDouble(orderAmount) };
            var status = new OrderStatusDetail { OrderId = orderId, CompanyKey = companyKey, AccountId = accountId, IBSOrderId = 12345 };

            ConfigurationManager.SetPaymentSettings(null, new ServerPaymentSettings { PaymentMode = PaymentMethod.Cmt });
            ConfigurationManager.SetPaymentSettings(companyKey, new ServerPaymentSettings { PaymentMode = PaymentMethod.Cmt });

            EnsurePreAuthForFeeWasCalled(status, bookingFees);
            EnsureCommitForFeeWasCalled(status, bookingFees, bookingFees);

            EnsurePreAuthPaymentForTripWasCalled(status, orderAmount + tip);
            EnsureCommitWasCalled(status, orderAmount + tip, orderAmount + tip, orderAmount, tip);

            // Act
            Sut.Update(ibsOrder, status);

            // Assert
            PaymentServiceMock.Verify();

            Assert.AreEqual(3, Commands.Count);
            Assert.AreEqual(typeof(CaptureCreditCardPayment), Commands.First().GetType());
            Assert.AreEqual(typeof(CaptureCreditCardPayment), Commands.Skip(1).First().GetType());
            Assert.AreEqual(typeof(ChangeOrderStatus), Commands.Skip(2).First().GetType());
        }

        [Test]
        public void when_order_on_external_company_has_booking_fees_configured_with_cmt_and_card_is_declined()
        {
            // Prepare
            var creditCardId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            string companyKey = "ext";
            var orderAmount = 100.85m;
            var bookingFees = 40.85m;

            using (var context = new BookingDbContext(DbName))
            {
                context.RemoveAll<FeesDetail>();
                context.SaveChanges();

                context.Save(new AccountDetail
                {
                    Id = accountId,
                    CreationDate = DateTime.Now,
                    IBSAccountId = 123,
                    DefaultCreditCard = creditCardId
                });

                context.Save(new AccountIbsDetail
                {
                    CompanyKey = companyKey,
                    AccountId = accountId,
                    IBSAccountId = 123
                });

                context.Save(new CreditCardDetails
                {
                    AccountId = accountId,
                    CreditCardId = creditCardId,
                    Token = "token"
                });

                context.Save(new OrderDetail
                {
                    Id = orderId,
                    AccountId = accountId,
                    PickupDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    IBSOrderId = 12345,
                    CompanyKey = companyKey,
                    BookingFees = bookingFees
                });

                context.Save(new OrderPairingDetail
                {
                    OrderId = orderId
                });

                context.Save(new FeesDetail
                {
                    Market = null,
                    Booking = bookingFees,
                    Id = Guid.NewGuid()
                });
            }

            var tip = FareHelper.CalculateTipAmount(orderAmount, ConfigurationManager.ServerData.DefaultTipPercentage);

            var ibsOrder = new IBSOrderInformation { Fare = Convert.ToDouble(orderAmount) };
            var status = new OrderStatusDetail { OrderId = orderId, CompanyKey = companyKey, AccountId = accountId, IBSOrderId = 12345 };

            ConfigurationManager.SetPaymentSettings(null, new ServerPaymentSettings { PaymentMode = PaymentMethod.Cmt });
            ConfigurationManager.SetPaymentSettings(companyKey, new ServerPaymentSettings { PaymentMode = PaymentMethod.Cmt });

            EnsurePreAuthForFeeWasCalled(status, bookingFees);

            EnsurePreAuthPaymentForTripWasCalled(status, orderAmount + tip);

            // Act
            Sut.Update(ibsOrder, status);

            // Wait for commands to be sent properly
            Thread.Sleep(5000);

            // Assert
            PaymentServiceMock.Verify();

            Assert.AreEqual(3, Commands.Count);
            Assert.AreEqual(typeof(ReactToPaymentFailure), Commands.First().GetType());
            Assert.AreEqual(typeof(ReactToPaymentFailure), Commands.Skip(1).First().GetType());
            Assert.AreEqual(typeof(ChangeOrderStatus), Commands.Skip(2).First().GetType());
        }
    }
}