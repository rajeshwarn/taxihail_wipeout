#region

using System;
using System.Linq;
using System.Net.Mail;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Email;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Booking.Test.Integration;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using MK.Common.Configuration;
using Moq;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.OrderFixture
{
    public class given_an_email_address : given_a_read_model_database
    {
        private const string ApplicationName = "TestApplication";
        private TestConfigurationManager _configurationManager;
        private Mock<IEmailSender> _emailSenderMock;
        private EventSourcingTestHelper<Order> sut;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new AccountDetail
                {
                    Id = Guid.NewGuid(),
                    Email = "test@example.net",
                    CreationDate = DateTime.Now
                });
            }

            using (var context = new ConfigurationDbContext(DbName))
            {
                context.Save(new NotificationSettings
                {
                    Id = AppConstants.CompanyId,
                    Enabled = true,
                    BookingConfirmationEmail = true,
                    ConfirmPairingPush = true,
                    DriverAssignedPush = true,
                    PaymentConfirmationPush = true,
                    NearbyTaxiPush = true,
                    ReceiptEmail = true,
                    VehicleAtPickupPush = true
                });
            }
        }

        [SetUp]
        public void Setup()
        {
            sut = new EventSourcingTestHelper<Order>();

            _emailSenderMock = new Mock<IEmailSender>();
            _configurationManager = new TestConfigurationManager();
            _configurationManager.SetSetting("TaxiHail.ApplicationName", ApplicationName);

            sut.Setup(new EmailCommandHandler(new NotificationService(() => new BookingDbContext(DbName), null, new TemplateService(_configurationManager), _emailSenderMock.Object, _configurationManager, _configurationManager, new ConfigurationDao(() => new ConfigurationDbContext(DbName)), null)));
        }

        [Test]
        public void when_sending_receipt_email()
        {
            sut.When(new SendReceipt
            {
                EmailAddress = "test@example.net",
                IBSOrderId = 777,
                VehicleNumber = "555",
                Fare = 26.32,
                Toll = 3.68,
                Tip = 5.25,
                Tax = 2.21,
                PickupAddress = new Address
                {
                    FullAddress = "5250, rue Ferrier, Montreal, H1P 4L4"
                },
                ClientLanguageCode = "fr"
            });

            _emailSenderMock.Verify(s => s
                .Send(It.Is<MailMessage>(message =>
                    message.AlternateViews.Any() &&
                    message.Subject.Contains(ApplicationName))));
        }

        [Test]
        public void given_cc_payment_when_sending_receipt_email()
        {
            sut.When(new SendReceipt
            {
                EmailAddress = "test@example.net",
                IBSOrderId = 777,
                VehicleNumber = "555",
                Fare = 26.32,
                Toll = 3.68,
                Tip = 5.25,
                Tax = 2.21,
                CardOnFileInfo = new SendReceipt.CardOnFile(22, 12354 + "qweqw", "1234", "Visa")
                {
                    LastFour = "6578",
                    NameOnCard = "Bob"
                },
                PickupAddress = new Address
                {
                    FullAddress = "5250, rue Ferrier, Montreal, H1P 4L4"
                },
                ClientLanguageCode = "fr"
            });

            _emailSenderMock.Verify(s => s
                .Send(It.Is<MailMessage>(message =>
                    message.AlternateViews.Any() &&
                    message.Subject.Contains(ApplicationName))));
        }

        [Test]
        public void given_paypal_payment_when_sending_receipt_email()
        {
            sut.When(new SendReceipt
            {
                EmailAddress = "test@example.net",
                IBSOrderId = 777,
                VehicleNumber = "555",
                Fare = 26.32,
                Toll = 3.68,
                Tip = 5.25,
                Tax = 2.21,
                CardOnFileInfo = new SendReceipt.CardOnFile(22, 12354 + "qweqw", "1231", "PayPal"),
                PickupAddress = new Address
                {
                    FullAddress = "5250, rue Ferrier, Montreal, H1P 4L4"
                },
                ClientLanguageCode = "fr"
            });

            _emailSenderMock.Verify(s => s
                .Send(It.Is<MailMessage>(message =>
                    message.AlternateViews.Any() &&
                    message.Subject.Contains(ApplicationName))));
        }
    }
}