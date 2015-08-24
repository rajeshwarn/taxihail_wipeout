using System;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.Test;
using apcurium.MK.Common;
using apcurium.MK.Events.Migration;
using apcurium.MK.Events.Migration.Projections;
using NUnit.Framework;

namespace MK.Events.Migration.Tests
{
    [TestFixture]
    public class AccountDetailsMigratorFixture : given_a_read_model_database
    {
        private TestServerSettings _testServerSettings;

        public AccountDetailsMigratorFixture()
        {
            _testServerSettings = new TestServerSettings();
            Sut = new AccountDetailsMigrator(_testServerSettings, () => new BookingDbContext(DbName));
        }

        public AccountDetailsMigrator Sut { get; set; }


        [Test]
        public void given_account_registered_event_with_missing_value_then_populated()
        {
            var @event = new AccountRegistered();
            

            @event = Sut.Migrate(@event);

            Assert.AreEqual("US", @event.Country.Code);
            Assert.AreEqual(_testServerSettings.ServerData.DefaultBookingSettings.NbPassenger, @event.NbPassengers);
            
        }

        [Test]
        public void given_account_registered_event_with_values_then_not_updated()
        {
            var @event = new AccountRegistered
            {
                 Country = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode("FR")).CountryISOCode,
                 NbPassengers = 8596
            };

            @event = Sut.Migrate(@event);

            Assert.AreEqual("FR", @event.Country.Code);
            Assert.AreEqual(8596, @event.NbPassengers);

        }

        [Test]
        public void given_credit_card_desactivated_event_with_missing_value_then_populated()
        {
            _testServerSettings.GetPaymentSettings().IsOutOfAppPaymentDisabled = true;
            var @event = new CreditCardDeactivated();

            @event = Sut.Migrate(@event);

            Assert.AreEqual(_testServerSettings.GetPaymentSettings().IsOutOfAppPaymentDisabled, @event.IsOutOfAppPaymentDisabled);

        }

        [Test]
        public void given_credit_card_desactivated_event_with_value_then_not_updated()
        {
            _testServerSettings.GetPaymentSettings().IsOutOfAppPaymentDisabled = true;
            var @event = new CreditCardDeactivated
            {
                IsOutOfAppPaymentDisabled = false
            };

            @event = Sut.Migrate(@event);

            Assert.AreEqual(false, @event.IsOutOfAppPaymentDisabled);

        }

        [Test]
        public void given_overdue_payment_settled_event_with_missing_value_then_populated()
        {
            _testServerSettings.GetPaymentSettings().IsPayInTaxiEnabled = true;
            var @event = new OverduePaymentSettled();

            @event = Sut.Migrate(@event);

            Assert.AreEqual(_testServerSettings.GetPaymentSettings().IsOutOfAppPaymentDisabled, @event.IsPayInTaxiEnabled);

        }

        [Test]
        public void given_overdue_payment_settled_event_with_value_then_not_updated()
        {
            _testServerSettings.GetPaymentSettings().IsPayInTaxiEnabled = true;
            var @event = new OverduePaymentSettled
            {
                IsPayInTaxiEnabled = false
            };

            @event = Sut.Migrate(@event);

            Assert.AreEqual(false, @event.IsPayInTaxiEnabled);

        }

        [Test]
        public void given_booking_settings_updated_event_with_values_then_set_null()
        {
            var @event = new BookingSettingsUpdated
            {
                ChargeTypeId = _testServerSettings.ServerData.DefaultBookingSettings.ChargeTypeId,
                VehicleTypeId = _testServerSettings.ServerData.DefaultBookingSettings.VehicleTypeId,
                ProviderId = _testServerSettings.ServerData.DefaultBookingSettings.ProviderId
            };

            @event = Sut.Migrate(@event);

            Assert.IsNull(@event.ChargeTypeId);
            Assert.IsNull(@event.VehicleTypeId);
            Assert.IsNull(@event.ProviderId);

            Assert.AreEqual("US", @event.Country.Code);

        }

        [Test]
        public void given_booking_settings_updated_event_with_no_default_values_then_not_set_null()
        {
            var @event = new BookingSettingsUpdated
            {
                ChargeTypeId = 1 + _testServerSettings.ServerData.DefaultBookingSettings.ChargeTypeId,
                VehicleTypeId = 1 + _testServerSettings.ServerData.DefaultBookingSettings.VehicleTypeId,
                ProviderId = 1 + _testServerSettings.ServerData.DefaultBookingSettings.ProviderId,
                Country = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode("FR")).CountryISOCode
            };

            @event = Sut.Migrate(@event);

            Assert.IsNotNull(@event.ChargeTypeId);
            Assert.IsNotNull(@event.VehicleTypeId);
            Assert.IsNotNull(@event.ProviderId);

            Assert.AreEqual("FR", @event.Country.Code);

        }

        [Test]
        public void given_credit_card_removed_event_with_missing_value_then_populated()
        {
            var accountId = Guid.NewGuid();
            var creditCardId = Guid.NewGuid();
            var newCCId = Guid.NewGuid();

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new CreditCardDetails
                {
                    AccountId = accountId,
                    CreditCardId = creditCardId
                });

                
                context.Save(new CreditCardDetails
                {
                    AccountId = accountId,
                    CreditCardId = newCCId
                });
            }
            var @event = new CreditCardRemoved
            {
                CreditCardId = creditCardId,
                SourceId = accountId
            };

            @event = Sut.Migrate(@event);

            Assert.AreEqual(newCCId, @event.NewDefaultCreditCardId);

        }

        [Test]
        public void given_credit_card_removed_event_with_value_then_not_updated()
        {
            var accountId = Guid.NewGuid();
            var creditCardId = Guid.NewGuid();

            using (var context = new BookingDbContext(DbName))
            {
                context.Save(new CreditCardDetails
                {
                    AccountId = accountId,
                    CreditCardId = creditCardId
                });
            }
            var @event = new CreditCardRemoved
            {
                NewDefaultCreditCardId = Guid.NewGuid(),
                SourceId = accountId
            };

            @event = Sut.Migrate(@event);

            Assert.AreNotEqual(creditCardId, @event.NewDefaultCreditCardId);

        }

    }
}
