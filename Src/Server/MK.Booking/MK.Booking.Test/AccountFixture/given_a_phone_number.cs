using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Maps.Impl;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Booking.SMS;
using Moq;
using NUnit.Framework;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Test.AccountFixture
{
    [TestFixture]
    public class given_a_phone_number
    {
        private const string ApplicationName = "TestApplication";
        private TestServerSettings _serverSettings;
        private Mock<ISmsService> _smsSenderMock;
        private Mock<IOrderDao> _orderDaoMock;
        private EventSourcingTestHelper<Account> _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new EventSourcingTestHelper<Account>();

            _smsSenderMock = new Mock<ISmsService>();

            _orderDaoMock = new Mock<IOrderDao>();
            _serverSettings = new TestServerSettings();
            _serverSettings.SetSetting("TaxiHail.ApplicationName", ApplicationName);
            _serverSettings.SetSetting("SMSConfirmationEnabled", "true");
            _serverSettings.SetSetting("SMSAccountSid", "AC081ad5bacfad6c50e0598052fc062693");
            _serverSettings.SetSetting("SMSAuthToken", "9b142e2d163a5688ada040d8c71e3fb1");
            _serverSettings.SetSetting("SMSFromNumber", "15147002781");

            _sut.Setup(new SmsCommandHandler(new NotificationService(null, null, null, null, _serverSettings, null, _orderDaoMock.Object, null, new StaticMap(), _smsSenderMock.Object, null, null, null)));
        }

        [Test]
        public void when_sending_confirmation_sms()
        {
            const string phoneNumber = "5145555555";
            const string activationCode = "12345";

            _sut.When(new SendAccountConfirmationSMS
            {
                ClientLanguageCode = "fr",
                CountryCode = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode("CA")).CountryISOCode,
                Code = activationCode,
                PhoneNumber = phoneNumber
            });

            var toPhoneNumber = new libphonenumber.PhoneNumber
            {
                CountryCode = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode("CA")).CountryDialCode,
                NationalNumber = long.Parse(phoneNumber)
            };

            _smsSenderMock.Verify(s => s.Send(
                It.Is<libphonenumber.PhoneNumber>(x => x.CountryCode == toPhoneNumber.CountryCode && x.NationalNumber == toPhoneNumber.NationalNumber), 
                It.Is<string>(message => message.Contains(activationCode))));
        }
    }
}
