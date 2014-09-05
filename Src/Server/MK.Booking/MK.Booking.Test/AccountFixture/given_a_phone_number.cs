using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Maps.Impl;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Booking.SMS;
using Moq;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.AccountFixture
{
    [TestFixture]
    public class given_a_phone_number
    {
        private const string ApplicationName = "TestApplication";
        private TestConfigurationManager _configurationManager;
        private Mock<ISmsService> _smsSenderMock;
        private EventSourcingTestHelper<Account> _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new EventSourcingTestHelper<Account>();

            _smsSenderMock = new Mock<ISmsService>();
            _configurationManager = new TestConfigurationManager();
            _configurationManager.SetSetting("TaxiHail.ApplicationName", ApplicationName);
            _configurationManager.SetSetting("Client.SMSConfirmationEnabled", "true");
            _configurationManager.SetSetting("SMSAccountSid", "AC081ad5bacfad6c50e0598052fc062693");
            _configurationManager.SetSetting("SMSAuthToken", "9b142e2d163a5688ada040d8c71e3fb1");
            _configurationManager.SetSetting("SMSFromNumber", "15147002781");

            _sut.Setup(new SmsCommandHandler(new NotificationService(null, null, null, null, _configurationManager, _configurationManager, null, null, new StaticMap(), _smsSenderMock.Object, null)));
        }

        [Test]
        public void when_sending_confirmation_sms()
        {
            const string phoneNumber = "15145555555";
            const string activationCode = "12345";

            _sut.When(new SendAccountConfirmationSMS
            {
                ClientLanguageCode = "fr",
                Code = activationCode,
                PhoneNumber = phoneNumber
            });

            _smsSenderMock.Verify(s => s
                .Send(phoneNumber, It.Is<string>(message =>
                    message.Contains(activationCode))));
        }
    }
}
