#region

using System.Linq;
using System.Net.Mail;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Email;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Common.Entity;
using Moq;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.OrderFixture
{
    public class given_an_email_address
    {
        private const string ApplicationName = "TestApplication";
        private TestConfigurationManager _configurationManager;
        private Mock<IEmailSender> _emailSenderMock;
        private EventSourcingTestHelper<Order> sut;

        [SetUp]
        public void Setup()
        {
            sut = new EventSourcingTestHelper<Order>();

            _emailSenderMock = new Mock<IEmailSender>();
            _configurationManager = new TestConfigurationManager();
            _configurationManager.SetSetting("TaxiHail.ApplicationName", ApplicationName);

            sut.Setup(new EmailCommandHandler(new NotificationService(() => null, null, new TemplateService(_configurationManager), _emailSenderMock.Object, _configurationManager, _configurationManager)));
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