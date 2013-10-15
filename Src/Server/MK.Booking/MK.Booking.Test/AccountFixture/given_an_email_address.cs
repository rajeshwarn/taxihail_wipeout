using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Moq;
using NUnit.Framework;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Email;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Test.AccountFixture
{
    public class given_an_email_address
    {
        private EventSourcingTestHelper<Account> sut;
        private Guid _accountId = Guid.NewGuid();
        private Mock<IEmailSender> emailSenderMock;
        private TestConfigurationManager configurationManager;
        const string ApplicationName = "TestApplication";

        [SetUp]
        public void Setup()
        {
            this.sut = new EventSourcingTestHelper<Account>();

            emailSenderMock = new Mock<IEmailSender>();
            configurationManager = new TestConfigurationManager();
            configurationManager.SetSetting("TaxiHail.ApplicationName", ApplicationName);

            this.sut.Setup(new EmailCommandHandler(configurationManager, new TemplateService(), emailSenderMock.Object));
        }

        [Test]
        public void when_sending_password_email()
        {
            const string newPassword = "123456";

            this.sut.When(new SendPasswordResetEmail { EmailAddress = "test@example.net", Password = newPassword });

            emailSenderMock.Verify(s => s
                .Send(It.Is<MailMessage>(message =>
                    message.AlternateViews[0] != null &&
                    message.Subject.Contains(ApplicationName))));

        }

        [Test]
        public void when_sending_confirmation_email()
        {
            var confirmationUrl = new Uri("http://example.net", UriKind.Absolute);
            this.sut.When(new SendAccountConfirmationEmail { EmailAddress = "test@example.net", ConfirmationUrl = confirmationUrl});

            emailSenderMock.Verify(s => s
                .Send(It.Is<MailMessage>(message => 
                    message.AlternateViews[0] != null &&
                    message.Subject.Contains(ApplicationName))));
        }

        [Test]
        public void when_sending_booking_confirmation_email()
        {
            this.sut.When(new SendBookingConfirmationEmail
                {
                    EmailAddress = "test@example.net",
                    DropOffAddress = new Address(),
                    PickupAddress = new Address(),
                    IBSOrderId = 12345,
                    Id = Guid.NewGuid(),
                    Note = "Tomato Sandwich",
                    PickupDate = DateTime.Now,
                    Settings = new SendBookingConfirmationEmail.BookingSettings()
                    
                });

            emailSenderMock.Verify(s => s
                .Send(It.Is<MailMessage>(message =>
                    message.AlternateViews[0] != null &&
                    message.Subject.Contains(ApplicationName))));
        }

     

        [Test]
        public void when_sending_driver_assigned_confirmation_email()
        {
            this.sut.When(new SendAssignedConfirmation
                {
                    EmailAddress = "test@example.net",
                    DropOffAddress = new Address(),
                    PickupAddress = new Address(),
                    IBSOrderId = 12345,
                    Id = Guid.NewGuid(),
                    PickupDate = DateTime.Now,
                    Settings = new SendBookingConfirmationEmail.BookingSettings(),
                    Fare = 12,
                    TransactionDate = DateTime.Now,
                    VehicleNumber = 12345+"Tony"
                    
                });

            emailSenderMock.Verify(s => s
                .Send(It.Is<MailMessage>(message =>
                    message.AlternateViews[0] != null &&
                    message.Subject.Contains(ApplicationName))));
        }

    }
}
