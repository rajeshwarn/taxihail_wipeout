﻿#region

using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.EventHandlers
{
    public class AccountDetailsGenerator :
        IEventHandler<AccountRegistered>,
        IEventHandler<AccountConfirmed>,
        IEventHandler<AccountDisabled>,
        IEventHandler<AccountUpdated>,
        IEventHandler<BookingSettingsUpdated>,
        IEventHandler<AccountPasswordReset>,
        IEventHandler<AccountPasswordUpdated>,
        IEventHandler<RoleAddedToUserAccount>,
        IEventHandler<PaymentProfileUpdated>,
        IEventHandler<CreditCardAdded>,
        IEventHandler<CreditCardRemoved>,
        IEventHandler<AllCreditCardsRemoved>
    {
        private readonly IServerSettings _serverSettings;
        private readonly Func<BookingDbContext> _contextFactory;

        public AccountDetailsGenerator(Func<BookingDbContext> contextFactory, IServerSettings serverSettings)
        {
            _serverSettings = serverSettings;
            _contextFactory = contextFactory;
        }

        public void Handle(AccountConfirmed @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.IsConfirmed = true;
                account.DisabledByAdmin = false;
                context.Save(account);
            }
        }

        public void Handle(AccountDisabled @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.IsConfirmed = false;
                account.DisabledByAdmin = true;
                context.Save(account);
            }
        }

        public void Handle(AccountPasswordReset @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.Password = @event.Password;
                context.Save(account);
            }
        }

        public void Handle(AccountPasswordUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.Password = @event.Password;
                context.Save(account);
            }
        }

        public void Handle(AccountRegistered @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = new AccountDetail
                {
                    Name = @event.Name,
                    Email = @event.Email,
                    Password = @event.Password,
                    Id = @event.SourceId,
                    IBSAccountId = @event.IbsAcccountId,
                    FacebookId = @event.FacebookId,
                    TwitterId = @event.TwitterId,
                    Language = @event.Language,
                    CreationDate = @event.EventDate,
                    ConfirmationToken = @event.ConfirmationToken,
                    IsConfirmed = @event.AccountActivationDisabled
                };

                if (@event.IsAdmin)
                {
                    account.Roles |= (int) Roles.Admin;
                }

                account.Settings = new BookingSettings
                {
                    Name = account.Name,
                    NumberOfTaxi = 1,
                    Passengers = _serverSettings.ServerData.DefaultBookingSettings.NbPassenger,
                    Phone = @event.Phone,
                };

                context.Save(account);
                var defaultCompanyAddress = (from a in context.Query<DefaultAddressDetails>()
                    select a).ToList();

                //add default company favorite address
                defaultCompanyAddress.ForEach(c => context.Set<AddressDetails>().Add(new AddressDetails
                {
                    AccountId = account.Id,
                    Apartment = c.Apartment,
                    BuildingName = c.BuildingName,
                    FriendlyName = c.FriendlyName,
                    FullAddress = c.FullAddress,
                    Id = Guid.NewGuid(),
                    IsHistoric = false,
                    Latitude = c.Latitude,
                    Longitude = c.Longitude,
                    RingCode = c.RingCode
                }));
                context.SaveChanges();
            }
        }

        public void Handle(AccountUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.Name = @event.Name;
                context.Save(account);
            }
        }

        public void Handle(BookingSettingsUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                var settings = account.Settings ?? new BookingSettings();
                settings.Name = @event.Name;

                settings.ChargeTypeId = @event.ChargeTypeId;
                settings.ProviderId = @event.ProviderId;
                settings.VehicleTypeId = @event.VehicleTypeId;

                if (settings.ChargeTypeId == _serverSettings.ServerData.DefaultBookingSettings.ChargeTypeId)
                {
                    settings.ChargeTypeId = null;
                }

                if (settings.VehicleTypeId == _serverSettings.ServerData.DefaultBookingSettings.VehicleTypeId)
                {
                    settings.VehicleTypeId = null;
                }

                if (settings.ProviderId == _serverSettings.ServerData.DefaultBookingSettings.ProviderId)
                {
                    settings.ProviderId = null;
                }

                settings.NumberOfTaxi = @event.NumberOfTaxi;
                settings.Passengers = @event.Passengers;
                settings.Phone = @event.Phone;
                settings.AccountNumber = @event.AccountNumber;

                account.Settings = settings;
                context.Save(account);
            }
        }

        public void Handle(PaymentProfileUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.DefaultCreditCard = @event.DefaultCreditCard;
                account.DefaultTipPercent = @event.DefaultTipPercent;
                context.Save(account);
            }
        }

        public void Handle(RoleAddedToUserAccount @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.Roles |= (int) Enum.Parse(typeof (Roles), @event.RoleName);
                context.Save(account);
            }
        }

        private static int? ParseToNullable(string val)
        {
            int result;
            return int.TryParse(val, out result) ? result : default(int?);
        }

        public void Handle(CreditCardAdded @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.Settings.ChargeTypeId = ChargeTypes.CardOnFile.Id;
                context.Save(account);
            }
        }

        public void Handle(CreditCardRemoved @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                // used for migration, if user removed one card but had another one, we set this one as the default card
                var account = context.Find<AccountDetail>(@event.SourceId);
                var otherCreditCardForAccount = context.Query<CreditCardDetails>().FirstOrDefault(x => x.AccountId == @event.SourceId && x.CreditCardId != @event.CreditCardId);
                account.DefaultCreditCard = otherCreditCardForAccount != null ? otherCreditCardForAccount.CreditCardId : (Guid?) null;
                account.Settings.ChargeTypeId = otherCreditCardForAccount != null ? ChargeTypes.CardOnFile.Id : ChargeTypes.PaymentInCar.Id;
                context.Save(account);
            }
        }

        public void Handle(AllCreditCardsRemoved @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.DefaultCreditCard = null;
                account.Settings.ChargeTypeId = ChargeTypes.PaymentInCar.Id;
                context.Save(account);
            }
        }
    }
}