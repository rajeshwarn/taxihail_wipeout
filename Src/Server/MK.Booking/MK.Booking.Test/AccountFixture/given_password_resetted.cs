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

namespace apcurium.MK.Booking.Test.AccountFixture
{
    public class given_password_resetted
    {
        private EventSourcingTestHelper<Account> sut;
        private Guid _accountId = Guid.NewGuid();
        private Mock<IEmailSender> emailSenderMock;

        [SetUp]
        public void Setup()
        {
            this.sut = new EventSourcingTestHelper<Account>();

            emailSenderMock = new Mock<IEmailSender>();

            this.sut.Setup(new EmailCommandHandler(new TestConfigurationManager(), new TemplateService(), emailSenderMock.Object));
        }

        [Test]
        public void when_sending_password_email()
        {
            const string newPassword = "123456";

            this.sut.When(new SendPasswordResettedEmail { EmailAddress = "test@example.net", Password = newPassword });

            emailSenderMock.Verify(s => s
                .Send(It.Is<MailMessage>(message => message
                    .Body.Contains(newPassword))));
        }
    }
}
