using System;
using System.Linq;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
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

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new AccountDetail
                {
                    Id = accountId,
                    CreationDate = DateTime.Now,
                    IBSAccountId = 123
                });

                context.Save(new CreditCardDetails
                {
                    AccountId = accountId,
                    CreditCardId = Guid.NewGuid(),
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
            var status = new OrderStatusDetail { OrderId = orderId, CompanyKey = companyKey, AccountId = accountId };

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

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new AccountDetail
                {
                    Id = accountId,
                    CreationDate = DateTime.Now,
                    IBSAccountId = 123
                });

                context.Save(new CreditCardDetails
                {
                    AccountId = accountId,
                    CreditCardId = Guid.NewGuid(),
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
            var status = new OrderStatusDetail { OrderId = orderId, CompanyKey = companyKey, AccountId = accountId };

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

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new AccountDetail
                {
                    Id = accountId,
                    CreationDate = DateTime.Now,
                    IBSAccountId = 123
                });

                context.Save(new CreditCardDetails
                {
                    AccountId = accountId,
                    CreditCardId = Guid.NewGuid(),
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
            var status = new OrderStatusDetail { OrderId = orderId, CompanyKey = companyKey, AccountId = accountId };

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

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new AccountDetail
                {
                    Id = accountId,
                    CreationDate = DateTime.Now,
                    IBSAccountId = 123
                });

                context.Save(new CreditCardDetails
                {
                    AccountId = accountId,
                    CreditCardId = Guid.NewGuid(),
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
            var status = new OrderStatusDetail { OrderId = orderId, CompanyKey = companyKey, AccountId = accountId };

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
            string companyKey = null;
            var orderAmount = 100.55m;
            var preAuthAmount = 50m;

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new AccountDetail
                {
                    Id = accountId,
                    CreationDate = DateTime.Now,
                    IBSAccountId = 123
                });

                context.Save(new CreditCardDetails
                {
                    AccountId = accountId,
                    CreditCardId = Guid.NewGuid(),
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
            var status = new OrderStatusDetail { OrderId = orderId, CompanyKey = companyKey, AccountId = accountId };

            ConfigurationManager.SetPaymentSettings(null, new ServerPaymentSettings { PaymentMode = PaymentMethod.Braintree });

            EnsureCommitWasCalled(status, preAuthAmount, orderAmount + tip, orderAmount, tip);

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
            string companyKey = null;
            var orderAmount = 100.85m;
            var preAuthAmount = 50m;

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new AccountDetail
                {
                    Id = accountId,
                    CreationDate = DateTime.Now,
                    IBSAccountId = 123
                });

                context.Save(new CreditCardDetails
                {
                    AccountId = accountId,
                    CreditCardId = Guid.NewGuid(),
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
            var status = new OrderStatusDetail { OrderId = orderId, CompanyKey = companyKey, AccountId = accountId };

            ConfigurationManager.SetPaymentSettings(null, new ServerPaymentSettings { PaymentMode = PaymentMethod.Braintree });

            EnsureCommitWasCalled(status, preAuthAmount, orderAmount + tip, orderAmount, tip);

            // Act
            Sut.Update(ibsOrder, status);

            // Assert
            PaymentServiceMock.Verify();

            Assert.AreEqual(3, Commands.Count);
            Assert.AreEqual(typeof(LogCreditCardError), Commands.First().GetType());
            Assert.AreEqual(typeof(ReactToPaymentFailure), Commands.Skip(1).First().GetType());
            Assert.AreEqual(typeof(ChangeOrderStatus), Commands.Skip(2).First().GetType());
        }
    }
}