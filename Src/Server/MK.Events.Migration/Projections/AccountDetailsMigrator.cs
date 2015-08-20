﻿using System;
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
            if (@event.Country == null || (@event.Country != null && string.IsNullOrEmpty(@event.Country.Code)))
            {
                var currentCultureInfo = CultureInfo.GetCultureInfo(_serverSettings.ServerData.PriceFormat);
                var countryCode = (new RegionInfo(currentCultureInfo.LCID)).TwoLetterISORegionName;
                @event.Country = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(countryCode)).CountryISOCode;
            }

            @event.NbPassengers = _serverSettings.ServerData.DefaultBookingSettings.NbPassenger;

            return @event;
        }

        public CreditCardDeactivated Migrate(CreditCardDeactivated @event)
        {
            @event.IsOutOfAppPaymentDisabled = _serverSettings.GetPaymentSettings().IsOutOfAppPaymentDisabled;
            return @event;
        }

        public BookingSettingsUpdated Migrate(BookingSettingsUpdated @event)
        {
            if (@event.ChargeTypeId == _serverSettings.ServerData.DefaultBookingSettings.ChargeTypeId)
            {
                @event.ChargeTypeId = null;
            }

            if (@event.VehicleTypeId == _serverSettings.ServerData.DefaultBookingSettings.VehicleTypeId)
            {
                @event.VehicleTypeId = null;
            }

            if (@event.ProviderId == _serverSettings.ServerData.DefaultBookingSettings.ProviderId)
            {
                @event.ProviderId = null;
            }

            if (@event.Country == null || (@event.Country != null && string.IsNullOrEmpty(@event.Country.Code)))
            {
                var currentCultureInfo = CultureInfo.GetCultureInfo(_serverSettings.ServerData.PriceFormat);
                var countryCode = (new RegionInfo(currentCultureInfo.LCID)).TwoLetterISORegionName;
                @event.Country = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(countryCode)).CountryISOCode;
            }

            return @event;
        }

        public OverduePaymentSettled Migrate(OverduePaymentSettled @event)
        {
            @event.IsPayInTaxiEnabled = _serverSettings.GetPaymentSettings().IsPayInTaxiEnabled;
            return @event;
        }

        public CreditCardRemoved Migrate(CreditCardRemoved @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var otherCreditCardForAccount = context.Query<CreditCardDetails>()
                        .FirstOrDefault(x => x.AccountId == @event.SourceId 
                                        && x.CreditCardId != @event.CreditCardId);

                if (otherCreditCardForAccount != null)
                {
                    @event.NewDefaultCreditCardId = otherCreditCardForAccount.CreditCardId;
                }
                return @event;
            }
        }
    }
}