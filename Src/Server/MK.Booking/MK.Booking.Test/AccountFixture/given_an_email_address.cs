#region

using System;
using System.Net.Mail;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Email;
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

namespace apcurium.MK.Booking.Test.AccountFixture
{
    public class given_an_email_address : given_a_read_model_database
    {
        private const string ApplicationName = "TestApplication";
        private TestConfigurationManager _configurationManager;
        private Mock<IEmailSender> _emailSenderMock;
        private EventSourcingTestHelper<Account> _sut;

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
            _sut = new EventSourcingTestHelper<Account>();

            _emailSenderMock = new Mock<IEmailSender>();
            _configurationManager = new TestConfigurationManager();
            _configurationManager.SetSetting("TaxiHail.ApplicationName", ApplicationName);

            _sut.Setup(new EmailCommandHandler(new NotificationService(() => new BookingDbContext(DbName), null, new TemplateService(_configurationManager), _emailSenderMock.Object, _configurationManager, _configurationManager, new ConfigurationDao(() => new ConfigurationDbContext(DbName)), null, null, null)));
        }

        [Test]
        public void when_sending_password_email()
        {
            const string newPassword = "123456";

            _sut.When(new SendPasswordResetEmail {EmailAddress = "test@example.net", Password = newPassword, ClientLanguageCode = "fr"});

            _emailSenderMock.Verify(s => s
                .Send(It.Is<MailMessage>(message =>
                    message.AlternateViews[0] != null &&
                    message.Subject.Contains(ApplicationName))));
        }

        [Test]
        public void when_sending_confirmation_email()
        {
            var confirmationUrl = new Uri("http://example.net", UriKind.Absolute);
            _sut.When(new SendAccountConfirmationEmail
            {
                EmailAddress = "test@example.net",
                ConfirmationUrl = confirmationUrl,
                ClientLanguageCode = "fr"
            });

            _emailSenderMock.Verify(s => s
                .Send(It.Is<MailMessage>(message =>
                    message.AlternateViews[0] != null &&
                    message.Subject.Contains(ApplicationName))));
        }

        [Test]
        public void when_sending_booking_confirmation_email()
        {
            _sut.When(new SendBookingConfirmationEmail
            {
                EmailAddress = "test@example.net",
                DropOffAddress = new Address(),
                PickupAddress = new Address(),
                IBSOrderId = 12345,
                Id = Guid.NewGuid(),
                Note = "Tomato Sandwich",
                PickupDate = DateTime.Now,
                Settings = new SendBookingConfirmationEmail.BookingSettings(),
                ClientLanguageCode = "fr"
            });

            _emailSenderMock.Verify(s => s
                .Send(It.Is<MailMessage>(message =>
                    message.AlternateViews[0] != null &&
                    message.Subject.Contains(ApplicationName))));
        }
    }
}