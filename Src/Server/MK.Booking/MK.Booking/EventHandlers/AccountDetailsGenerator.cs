#region

using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
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
        IEventHandler<CreditCardAddedOrUpdated>,
        IEventHandler<CreditCardRemoved>,
        IEventHandler<AllCreditCardsRemoved>,
        IEventHandler<CreditCardDeactivated>,
        IEventHandler<AccountLinkedToIbs>,
        IEventHandler<AccountUnlinkedFromIbs>,
        IEventHandler<PayPalAccountLinked>,
        IEventHandler<PayPalAccountUnlinked>,
        IEventHandler<OverduePaymentSettled>,
        IEventHandler<ChargeAccountPaymentDisabled>
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public AccountDetailsGenerator(Func<BookingDbContext> contextFactory)
        {
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
                    Passengers = @event.NbPassengers,
                    Country = @event.Country,
                    Phone = @event.Phone,
                    PayBack = @event.PayBack
                };

                context.Save(account);
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

                settings.NumberOfTaxi = @event.NumberOfTaxi;
                settings.Passengers = @event.Passengers;
                settings.Country = @event.Country;
                settings.Phone = @event.Phone;
                settings.AccountNumber = @event.AccountNumber;
                settings.CustomerNumber = @event.CustomerNumber;
                settings.PayBack = @event.PayBack;

                account.DefaultTipPercent = @event.DefaultTipPercent;

                account.Settings = settings;
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

        public void Handle(CreditCardAddedOrUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.DefaultCreditCard = @event.CreditCardId;
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
                account.DefaultCreditCard = @event.NewDefaultCreditCardId;
                account.Settings.ChargeTypeId = @event.NewDefaultCreditCardId == null
                    ? ChargeTypes.PaymentInCar.Id
                    : ChargeTypes.CardOnFile.Id;
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
            if (!@event.IsOutOfAppPaymentDisabled)
            {
                using (var context = _contextFactory.Invoke())
                {
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
                if (!@event.CompanyKey.HasValue())
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
                context.Save(account);
            }
        }

        public void Handle(PayPalAccountLinked @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.IsPayPalAccountLinked = true;
                account.Settings.ChargeTypeId = ChargeTypes.PayPal.Id;
                context.Save(account);
            }
        }

        public void Handle(PayPalAccountUnlinked @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.IsPayPalAccountLinked = false;
                context.Save(account);
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
            if (@event.IsPayInTaxiEnabled)
            {
                using (var context = _contextFactory.Invoke())
                {
                    //Re-enable card on file as the default payment method
                    var account = context.Find<AccountDetail>(@event.SourceId);
                    account.Settings.ChargeTypeId = ChargeTypes.CardOnFile.Id;
                    context.Save(account);
                }
            }
        }

        
    }
}