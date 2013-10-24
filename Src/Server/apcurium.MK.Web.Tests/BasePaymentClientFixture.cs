﻿using System;
using System.Data.Entity;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using apcurium.MK.Web.Tests;

namespace apcurium.CMT.Web.Tests
{
    [TestFixture]
    public abstract class BasePaymentClientFixture : BaseTest
    {
        [TestFixtureSetUp]
        public override void TestFixtureSetup()
        {
            base.TestFixtureSetup();
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();
        }

        [SetUp]
        public override void Setup()
        {

            base.Setup();
            CreateAndAuthenticateTestAccount();            
        }
        protected BasePaymentClientFixture(TestCreditCards.TestCreditCardSetting settings)
        {
            TestCreditCards = new TestCreditCards(settings);
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MKWebDev"].ConnectionString;
            ContextFactory = () => new BookingDbContext(connectionString);
        }

        protected Func<DbContext> ContextFactory { get; set; }

        TestCreditCards TestCreditCards { get; set; }

        protected abstract IPaymentServiceClient GetPaymentClient();


        [Test]
        public void when_tokenizing_a_credit_card_visa()
        {
            var client = GetPaymentClient();
            var response = client.Tokenize(TestCreditCards.Visa.Number, TestCreditCards.Visa.ExpirationDate, TestCreditCards.Visa.AvcCvvCvv2 + "");
            Assert.True(response.IsSuccessfull, response.Message);
        }

        [Test]
        public void when_tokenizing_a_credit_card_mastercard()
        {
            var client = GetPaymentClient();
            var response = client.Tokenize(TestCreditCards.Mastercard.Number, TestCreditCards.Mastercard.ExpirationDate, TestCreditCards.Mastercard.AvcCvvCvv2 + "");
            Assert.True(response.IsSuccessfull, response.Message);
        }

        [Test]
        public void when_tokenizing_a_credit_card_amex()
        {
            var client = GetPaymentClient();

            var response = client.Tokenize(TestCreditCards.AmericanExpress.Number, TestCreditCards.AmericanExpress.ExpirationDate, TestCreditCards.AmericanExpress.AvcCvvCvv2 + "");
            Assert.True(response.IsSuccessfull, response.Message);
        }

        [Test]
        public void when_tokenizing_a_credit_card_discover()
        {
            var client = GetPaymentClient();

            var response = client.Tokenize(TestCreditCards.Discover.Number, TestCreditCards.Discover.ExpirationDate, TestCreditCards.Discover.AvcCvvCvv2 + "");
            Assert.True(response.IsSuccessfull, response.Message);
        }

        [Test]
        public void when_deleting_a_tokenized_credit_card()
        {
            var client = GetPaymentClient();


            var token = client.Tokenize(TestCreditCards.Visa.Number, TestCreditCards.Visa.ExpirationDate, TestCreditCards.Visa.AvcCvvCvv2 + "").CardOnFileToken;

            var response = client.ForgetTokenizedCard(token);
            Assert.True(response.IsSuccessfull, response.Message);
        }

        [Test]
        public void when_preauthorizing_a_credit_card_payment()
        {
            var orderId = Guid.NewGuid();
            using (var context = ContextFactory.Invoke())
            {
                context.Set<OrderDetail>().Add(new OrderDetail
                {
                    Id = orderId,
                    IBSOrderId = 1234,
                    CreatedDate = DateTime.Now,
                    PickupDate = DateTime.Now
                });
                context.SaveChanges();
            }
            var client = GetPaymentClient();

            var token = client.Tokenize(TestCreditCards.Mastercard.Number, TestCreditCards.Mastercard.ExpirationDate, TestCreditCards.Mastercard.AvcCvvCvv2 + "").CardOnFileToken;

            const double amount = 21.56;
            var response = client.PreAuthorize(token, amount, orderId);

            Assert.True(response.IsSuccessfull);

        }

        [Test]
        public void when_capturing_a_preauthorized_a_credit_card_payment()
        {
            var orderId = Guid.NewGuid();
            using (var context = ContextFactory.Invoke())
            {
                context.Set<OrderDetail>().Add(new OrderDetail
                {
                    Id = orderId,
                    IBSOrderId = 1234,
                    CreatedDate = DateTime.Now,
                    PickupDate = DateTime.Now
                });
                context.Set<OrderStatusDetail>().Add(new OrderStatusDetail
                {
                    OrderId = orderId,
                    VehicleNumber = "vehicle",
                    PickupDate = DateTime.Now,
                });
                context.SaveChanges();
            }

            var client = GetPaymentClient();

            var token = client.Tokenize(TestCreditCards.Discover.Number, TestCreditCards.Discover.ExpirationDate, TestCreditCards.Discover.AvcCvvCvv2 + "").CardOnFileToken;

            const double amount = 1.50;
            var authorization = client.PreAuthorize(token, amount, orderId);

            Assert.True(authorization.IsSuccessfull, authorization.Message);
            
            var response = client.CommitPreAuthorized(authorization.TransactionId);

            Assert.True(response.IsSuccessfull, response.Message);
        }


        [Test]
        public void when_authorized_a_credit_card_payment_and_resending_confirmation()
        {
            var orderId = Guid.NewGuid();
            using (var context = ContextFactory.Invoke())
            {
                context.Set<OrderDetail>().Add(new OrderDetail
                {
                    Id = orderId,
                    IBSOrderId = 1234,
                    CreatedDate = DateTime.Now,
                    PickupDate = DateTime.Now
                });
                context.Set<OrderStatusDetail>().Add(new OrderStatusDetail
                {
                    OrderId = orderId,
                    VehicleNumber = "vehicle",
                    PickupDate = DateTime.Now,
                });
                context.SaveChanges();
            }

            var client = GetPaymentClient();

            var token = client.Tokenize(TestCreditCards.Discover.Number, TestCreditCards.Discover.ExpirationDate, TestCreditCards.Discover.AvcCvvCvv2 + "").CardOnFileToken;

            const double amount = 1.50;
            var authorization = client.PreAuthorize(token, amount, orderId);

            Assert.True(authorization.IsSuccessfull, authorization.Message);

            var response = client.CommitPreAuthorized(authorization.TransactionId);

            Assert.True(response.IsSuccessfull, response.Message);

            client.ResendConfirmationToDriver(orderId);
        }

    }
}
