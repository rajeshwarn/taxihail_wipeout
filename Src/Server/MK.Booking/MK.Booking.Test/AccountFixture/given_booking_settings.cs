#region

using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Security;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.AccountFixture
{
    [TestFixture]
    public class given_booking_settings
    {
        [SetUp]
        public void Setup()
        {
            _sut = new EventSourcingTestHelper<Account>();
            _sut.Setup(new AccountCommandHandler(_sut.Repository, new PasswordService(), null, null));
            _sut.Given(new AccountRegistered
            {
                SourceId = _accountId,
                Name = "Bob Smith",
                Password = null,
                Email = "bob.smith@apcurium.com"
            });
        }

        private EventSourcingTestHelper<Account> _sut;
        private readonly Guid _accountId = Guid.NewGuid();

        [Test]
        public void when_updating_successfully()
        {
            Guid? creditCardId = Guid.NewGuid();
            int? defaultTipPercent = 15;

            _sut.When(new UpdateBookingSettings
            {
                AccountId = _accountId,
                Name = "Robert2 Smither2",
                ChargeTypeId = 12,
                NumberOfTaxi = 3,
                Phone = "123",
                Passengers = 3,
                ProviderId = 85,
                VehicleTypeId = 69,
                DefaultCreditCard = creditCardId,
                DefaultTipPercent = defaultTipPercent,
                AccountNumber = "1234"
            });

            var @event = _sut.ThenHasOne<BookingSettingsUpdated>();

            Assert.AreEqual(_accountId, @event.SourceId);
            Assert.AreEqual("Robert2 Smither2", @event.Name);
            Assert.AreEqual(12, @event.ChargeTypeId);
            Assert.AreEqual(3, @event.NumberOfTaxi);
            Assert.AreEqual("123", @event.Phone);
            Assert.AreEqual(3, @event.Passengers);
            Assert.AreEqual(85, @event.ProviderId);
            Assert.AreEqual(69, @event.VehicleTypeId);
            Assert.AreEqual("1234", @event.AccountNumber);

            var @eventPayment = _sut.ThenHasOne<PaymentProfileUpdated>();
            Assert.AreEqual(_accountId, @eventPayment.SourceId);
            Assert.AreEqual(creditCardId, @eventPayment.DefaultCreditCard);
            Assert.AreEqual(defaultTipPercent, @eventPayment.DefaultTipPercent);
        }
    }
}