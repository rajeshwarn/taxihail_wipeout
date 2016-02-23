using System;
using System.Linq;
using System.Net.Mail;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Email;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.Maps.Impl;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Booking.Test.Integration;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using CustomerPortal.Client;
using CustomerPortal.Contract.Response;
using Moq;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.OrderFixture
{
    public class given_an_email_address : given_a_read_model_database
    {
        private const string ApplicationName = "TestApplication";
        private TestServerSettings _serverSettings;
        private Mock<IEmailSender> _emailSenderMock;
        private Mock<IOrderDao> _orderDaoMock;
        private Mock<IAccountDao> _accountDaoMock;
        private Mock<IGeocoding> _geocodingMock;
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
                    PromotionUnlockedEmail = true,
                    VehicleAtPickupPush = true,
                    PromotionUnlockedPush = true,
                    UnpairingReminderPush = true,
                    DriverBailedPush = true,
                    NoShowPush = true
                });
            }
        }

        [SetUp]
        public void Setup()
        {
            sut = new EventSourcingTestHelper<Order>();

            _emailSenderMock = new Mock<IEmailSender>();
            _orderDaoMock = new Mock<IOrderDao>();
            _accountDaoMock = new Mock<IAccountDao>();
            _serverSettings = new TestServerSettings();
            _geocodingMock = new Mock<IGeocoding>();
            var taxihailNetworkServiceClientMock = new Mock<ITaxiHailNetworkServiceClient>();
            taxihailNetworkServiceClientMock
                .Setup(x => x.GetCompanyMarketSettings(It.IsAny<double>(), It.IsAny<double>()))
                .Returns(new CompanyMarketSettingsResponse());
            _serverSettings.SetSetting("TaxiHail.ApplicationName", ApplicationName);

            var notificationService = new NotificationService(() => new BookingDbContext(DbName),
                null,
                new TemplateService(_serverSettings),
                _emailSenderMock.Object,
                _serverSettings,
                new ConfigurationDao(() => new ConfigurationDbContext(DbName)),
                _orderDaoMock.Object,
                _accountDaoMock.Object,
                new StaticMap(),
                null,
                _geocodingMock.Object,
                taxihailNetworkServiceClientMock.Object,
                new Logger());
            notificationService.SetBaseUrl(new Uri("http://www.example.net"));

            sut.Setup(new EmailCommandHandler(notificationService));
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
                PaymentInfo = new SendReceipt.Payment(22, 12354 + "qweqw", "1234", "Visa")
                {
                    Last4Digits = "6578",
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
                PaymentInfo = new SendReceipt.Payment(22, 12354 + "qweqw", "1231", "PayPal"),
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