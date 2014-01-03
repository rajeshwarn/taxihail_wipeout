using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using apcurium.MK.Common.Entity;
using Moq;
using NUnit.Framework;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Email;

namespace apcurium.MK.Booking.Test.OrderFixture
{
    public class given_an_email_address
    {
        private EventSourcingTestHelper<Order> sut;
        private Guid _accountId = Guid.NewGuid();
        private Mock<IEmailSender> emailSenderMock;
        private TestConfigurationManager configurationManager;
        const string ApplicationName = "TestApplication";

        [SetUp]
        public void Setup()
        {
            this.sut = new EventSourcingTestHelper<Order>();

            emailSenderMock = new Mock<IEmailSender>();
            configurationManager = new TestConfigurationManager();
            configurationManager.SetSetting("TaxiHail.ApplicationName", ApplicationName);

            this.sut.Setup(new EmailCommandHandler(configurationManager, new TemplateService(), emailSenderMock.Object));
        }

        [Test]
        public void when_sending_receipt_email()
        {
            sut.When(new SendReceipt
            {
                EmailAddress = "test@example.net",
                IBSOrderId = 777,
                VehicleNumber = "Cab555",
                Fare = 26.32,
                Toll = 3.68,
                Tip = 5.25,
                Tax = 2.21,
                PickupAddress = new Address
                {
                    FullAddress = "5250, rue Ferrier, Montreal, H1P 4L4"
                }

            });

            emailSenderMock.Verify(s => s
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
                VehicleNumber = "Cab555",
                Fare = 26.32,
                Toll = 3.68,
                Tip = 5.25,
                Tax = 2.21,
                CardOnFileInfo = new SendReceipt.CardOnFile(22,12354+"qweqw","1234","Visa")
                {
                    LastFour = "6578",
                    FriendlyName = "Home"
                },
                PickupAddress = new Address
                {
                    FullAddress = "5250, rue Ferrier, Montreal, H1P 4L4"
                }

            });

            emailSenderMock.Verify(s => s
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
                VehicleNumber = "Cab555",
                Fare = 26.32,
                Toll = 3.68,
                Tip = 5.25,
                Tax = 2.21,
                CardOnFileInfo = new SendReceipt.CardOnFile(22, 12354 + "qweqw", "1231", "PayPal"),
                 PickupAddress = new Address
                {
                    FullAddress = "5250, rue Ferrier, Montreal, H1P 4L4"
                }

            });

            emailSenderMock.Verify(s => s
                .Send(It.Is<MailMessage>(message =>
                    message.AlternateViews.Any() &&
                    message.Subject.Contains(ApplicationName))));

        }
    }
}
