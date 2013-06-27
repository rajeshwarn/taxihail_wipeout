using System;
using NUnit.Framework;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Security;

namespace apcurium.MK.Booking.Test.AccountFixture
{
    [TestFixture]
    public class given_booking_settings
    {
        private EventSourcingTestHelper<Account> _sut;
        private readonly Guid _accountId = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            this._sut = new EventSourcingTestHelper<Account>();
            this._sut.Setup(new AccountCommandHandler(this._sut.Repository, new PasswordService()));
            this._sut.Given(new AccountRegistered { SourceId = _accountId, Name = "Bob Smith", Password = null, Email = "bob.smith@apcurium.com" });
        }

        [Test]
        public void when_updating_successfully()
        {
            Guid? creditCardId = Guid.NewGuid();
            int? defaultTipPercent = 15;

            this._sut.When(new UpdateBookingSettings
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
                                   DefaultTipPercent = defaultTipPercent
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

            var @eventPayment = _sut.ThenHasOne<PaymentProfileUpdated>();
            Assert.AreEqual(_accountId, @eventPayment.SourceId);
            Assert.AreEqual(creditCardId, @eventPayment.DefaultCreditCard);
            Assert.AreEqual(defaultTipPercent, @eventPayment.DefaultTipPercent);
        }
    }
}