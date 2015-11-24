using System;
using System.Linq;
using System.Globalization;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Events.Migration.Projections
{
    public class AccountDetailsMigrator : IMigrateEvent<AccountRegistered>,
        IMigrateEvent<CreditCardDeactivated>,
        IMigrateEvent<BookingSettingsUpdated>,
        IMigrateEvent<OverduePaymentSettled>,
        IMigrateEvent<CreditCardRemoved>
    {
        private readonly IServerSettings _serverSettings;
        private readonly Func<BookingDbContext> _contextFactory;

        public AccountDetailsMigrator(IServerSettings serverSettings, Func<BookingDbContext> contextFactory)
        {
            _serverSettings = serverSettings;
            _contextFactory = contextFactory;
        }

        public AccountRegistered Migrate(AccountRegistered @event)
        {
            var wasMigrated = false;
            if (@event.Country == null || (@event.Country != null && string.IsNullOrEmpty(@event.Country.Code)))
            {
                wasMigrated = true;
                var currentCultureInfo = CultureInfo.GetCultureInfo(_serverSettings.ServerData.PriceFormat);
                var countryCode = (new RegionInfo(currentCultureInfo.LCID)).TwoLetterISORegionName;
                @event.Country = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(countryCode)).CountryISOCode;
            }

            if (@event.NbPassengers == null)
            {
                wasMigrated = true;
                @event.NbPassengers = _serverSettings.ServerData.DefaultBookingSettings.NbPassenger;
            }

            return wasMigrated ? @event : null;
        }

        public CreditCardDeactivated Migrate(CreditCardDeactivated @event)
        {
            if (@event.IsOutOfAppPaymentDisabled == null)
            {
                @event.IsOutOfAppPaymentDisabled = _serverSettings.GetPaymentSettings().IsOutOfAppPaymentDisabled;
                return @event;
            }
            else
            {
                return null;
            }
            
        }

        public BookingSettingsUpdated Migrate(BookingSettingsUpdated @event)
        {
            var wasMigrated = false;
            if (@event.ChargeTypeId == _serverSettings.ServerData.DefaultBookingSettings.ChargeTypeId)
            {
                @event.ChargeTypeId = null;
                wasMigrated = true;
            }

            if (@event.VehicleTypeId == _serverSettings.ServerData.DefaultBookingSettings.VehicleTypeId)
            {
                @event.VehicleTypeId = null;
                wasMigrated = true;
            }

            if (@event.ProviderId == _serverSettings.ServerData.DefaultBookingSettings.ProviderId)
            {
                @event.ProviderId = null;
                wasMigrated = true;
            }

            if (@event.Country == null || (@event.Country != null && string.IsNullOrEmpty(@event.Country.Code)))
            {
                var currentCultureInfo = CultureInfo.GetCultureInfo(_serverSettings.ServerData.PriceFormat);
                var countryCode = (new RegionInfo(currentCultureInfo.LCID)).TwoLetterISORegionName;
                @event.Country = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(countryCode)).CountryISOCode;
                wasMigrated = true;
            }

            return wasMigrated ? @event : null;
        }

        public OverduePaymentSettled Migrate(OverduePaymentSettled @event)
        {
            if (@event.IsPayInTaxiEnabled == null)
            {
                @event.IsPayInTaxiEnabled = _serverSettings.GetPaymentSettings().IsPayInTaxiEnabled;
                return @event;
            }
            else
            {
                return null;
            }
            
        }

        public CreditCardRemoved Migrate(CreditCardRemoved @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var otherCreditCardForAccount = context.Query<CreditCardDetails>()
                        .FirstOrDefault(x => x.AccountId == @event.SourceId 
                                        && x.CreditCardId != @event.CreditCardId);

                if (otherCreditCardForAccount != null
                    && @event.NextDefaultCreditCardId == null)
                {
                    @event.NextDefaultCreditCardId = otherCreditCardForAccount.CreditCardId;
                    return @event;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}