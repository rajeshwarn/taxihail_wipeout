#region

using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using System.Globalization;
using apcurium.MK.Common;
using apcurium.MK.Common.Helpers;

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
        IEventHandler<RoleUpdatedToUserAccount>,
        IEventHandler<PaymentProfileUpdated>,
        IEventHandler<CreditCardAddedOrUpdated>,
        IEventHandler<DefaultCreditCardUpdated>,
        IEventHandler<CreditCardRemoved>,
        IEventHandler<AllCreditCardsRemoved>,
        IEventHandler<CreditCardDeactivated>,
        IEventHandler<AccountLinkedToIbs>,
        IEventHandler<AccountUnlinkedFromIbs>,
        IEventHandler<PayPalAccountLinked>,
        IEventHandler<PayPalAccountUnlinked>,
        IEventHandler<OverduePaymentSettled>,
        IEventHandler<ChargeAccountPaymentDisabled>,
        IEventHandler<AccountAnswersAddedUpdated>
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

                if (@event.Country == null || (@event.Country != null && string.IsNullOrEmpty(@event.Country.Code)))
                {
                    var currentCultureInfo = CultureInfo.GetCultureInfo(_serverSettings.ServerData.PriceFormat);
                    string countryCode = (new RegionInfo(currentCultureInfo.LCID)).TwoLetterISORegionName;
                    @event.Country = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(countryCode)).CountryISOCode;
                }

                account.Settings = new BookingSettings
                {
                    Name = account.Name,
                    NumberOfTaxi = 1,
                    Passengers = _serverSettings.ServerData.DefaultBookingSettings.NbPassenger,
                    Country = @event.Country,
                    Phone = @event.Phone,
                    PayBack = @event.PayBack
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

                if (@event.Country == null || (@event.Country != null && string.IsNullOrEmpty(@event.Country.Code)))
                {
                    var currentCultureInfo = CultureInfo.GetCultureInfo(_serverSettings.ServerData.PriceFormat);
                    string countryCode = (new RegionInfo(currentCultureInfo.LCID)).TwoLetterISORegionName;
                    @event.Country = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(countryCode)).CountryISOCode;
                }

                settings.NumberOfTaxi = @event.NumberOfTaxi;
                settings.Passengers = @event.Passengers;
                settings.Country = @event.Country ?? new CountryISOCode();
                settings.Phone = @event.Phone;
                settings.AccountNumber = @event.AccountNumber;
                settings.CustomerNumber = @event.CustomerNumber;
                settings.PayBack = @event.PayBack;
				account.Email = @event.Email;
                account.DefaultTipPercent = @event.DefaultTipPercent;

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

        public void Handle(RoleUpdatedToUserAccount @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.Roles = (int)Enum.Parse(typeof(Roles), @event.RoleName);
                context.Save(account);
            }
        }

        public void Handle(CreditCardAddedOrUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                if (!account.DefaultCreditCard.HasValue)
                {
                    account.DefaultCreditCard = @event.CreditCardId;
                    account.Settings.ChargeTypeId = ChargeTypes.CardOnFile.Id;
                }
                context.Save(account);
            }
        }

        public void Handle(DefaultCreditCardUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.DefaultCreditCard = @event.CreditCardId;
                context.Save(account);
            }
        }

        public void Handle(CreditCardRemoved @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                // used for migration, if user removed one card but had another one, we set this one as the default card
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.DefaultCreditCard = @event.NextDefaultCreditCardId;
                account.Settings.ChargeTypeId = @event.NextDefaultCreditCardId.HasValue ? ChargeTypes.CardOnFile.Id : ChargeTypes.PaymentInCar.Id;
                context.Save(account);
            }
        }

        public void Handle(AllCreditCardsRemoved @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.DefaultCreditCard = null;

                account.Settings.ChargeTypeId = account.IsPayPalAccountLinked
                    ? ChargeTypes.PayPal.Id
                    : ChargeTypes.PaymentInCar.Id;
                
                context.Save(account);
            }
        }

        public void Handle(CreditCardDeactivated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                if (!_serverSettings.GetPaymentSettings().IsOutOfAppPaymentDisabled)
                {
                    // If pay in taxi is not disable, this becomes the default payment method
                    var account = context.Find<AccountDetail>(@event.SourceId);
                    account.Settings.ChargeTypeId = ChargeTypes.PaymentInCar.Id;
                    context.Save(account);
                }
            }
        }

        public void Handle(AccountLinkedToIbs @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                if (@event.CompanyKey.HasValue())
                {
                    var ibsAccountLink = 
                        context.Query<AccountIbsDetail>().FirstOrDefault(x => x.AccountId == @event.SourceId && x.CompanyKey == @event.CompanyKey) 
                            ?? new AccountIbsDetail
                               {
                                   AccountId = @event.SourceId,
                                   CompanyKey = @event.CompanyKey
                               };

                    ibsAccountLink.IBSAccountId = @event.IbsAccountId;

                    context.Save(ibsAccountLink);
                }
                else
                {
                    var account = context.Find<AccountDetail>(@event.SourceId);
                    account.IBSAccountId = @event.IbsAccountId;
                    context.Save(account);
                }
            }
        }

        public void Handle(AccountUnlinkedFromIbs @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.IBSAccountId = null;

                context.RemoveWhere<AccountIbsDetail>(x => x.AccountId == @event.SourceId);
                context.SaveChanges();
            }
        }

        public void Handle(PayPalAccountLinked @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.IsPayPalAccountLinked = true;
                account.Settings.ChargeTypeId = ChargeTypes.PayPal.Id;

                var payPalAccountDetails = context.Find<PayPalAccountDetails>(@event.SourceId);
                if (payPalAccountDetails == null)
                {
                    context.Save(new PayPalAccountDetails
                    {
                        AccountId = @event.SourceId,
                        EncryptedRefreshToken = @event.EncryptedRefreshToken
                    });
                }
                else
                {
                    payPalAccountDetails.EncryptedRefreshToken = @event.EncryptedRefreshToken;
                }

                context.SaveChanges();
            }
        }

        public void Handle(PayPalAccountUnlinked @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.IsPayPalAccountLinked = false;
                context.Save(account);

                context.RemoveWhere<PayPalAccountDetails>(x => x.AccountId == @event.SourceId);
                context.SaveChanges();
            }
        }

        public void Handle(ChargeAccountPaymentDisabled @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var accounts = context.Set<AccountDetail>()
                    .Where(HasChargeAccount);

                foreach (var account in accounts)
                {
                    account.Settings.CustomerNumber = null;
                    account.Settings.AccountNumber = null;
                }
                
                context.SaveChanges();
            }
        }

        private bool HasChargeAccount(AccountDetail accountDetail)
        {
            return accountDetail.Settings.AccountNumber.HasValue() ||
                   accountDetail.Settings.CustomerNumber.HasValue();
        }

        public void Handle(OverduePaymentSettled @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                if (_serverSettings.GetPaymentSettings().IsPayInTaxiEnabled)
                {
                    // Re-enable card on file as the default payment method
                    var account = context.Find<AccountDetail>(@event.SourceId);
                    account.Settings.ChargeTypeId = ChargeTypes.CardOnFile.Id;
                    context.Save(account);
                }
            }
        }

        public void Handle(AccountAnswersAddedUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                @event.Answers.ForEach(x => {
                    var answer = context.Query<AccountChargeQuestionAnswer>()
                        .Where(a => a.AccountId == x.AccountId && a.AccountChargeQuestionId == x.AccountChargeQuestionId && a.AccountChargeId == x.AccountChargeId)
                        .FirstOrDefault();
                    if (answer == null) {
                        context.Save(x);
                    } else {
                        answer.LastAnswer = x.LastAnswer;
                        context.Save(answer);
                    }
                });
            }
        }
    }
}