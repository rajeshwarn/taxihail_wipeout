using System;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Common.Configuration.Impl;

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
        IEventHandler<ChargeAccountPaymentDisabled>
    {
        private readonly IProjectionSet<AccountDetail> _projections;

        public AccountDetailsGenerator(IProjectionSet<AccountDetail> projections)
        {
            _projections = projections;
        }

        public void Handle(AccountConfirmed @event)
        {
            _projections.Update(@event.SourceId, account =>
            {
                account.IsConfirmed = true;
                account.DisabledByAdmin = false;
            });
        }

        public void Handle(AccountDisabled @event)
        {
            _projections.Update(@event.SourceId, account =>
            {
                account.IsConfirmed = false;
                account.DisabledByAdmin = true;
            });
        }

        public void Handle(AccountPasswordReset @event)
        {
            _projections.Update(@event.SourceId, account =>
            {
                account.Password = @event.Password;
            });
        }

        public void Handle(AccountPasswordUpdated @event)
        {
            _projections.Update(@event.SourceId, account =>
            {
                account.Password = @event.Password;
            });
        }

        public void Handle(AccountRegistered @event)
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
                Passengers = @event.NbPassengers.Value,
                Country = @event.Country,
                Phone = @event.Phone,
                PayBack = @event.PayBack
            };

            _projections.Add(account);
        }

        public void Handle(AccountUpdated @event)
        {
            _projections.Update(@event.SourceId, account =>
            {
                account.Name = @event.Name;
            });
        }

        public void Handle(BookingSettingsUpdated @event)
        {
            _projections.Update(@event.SourceId, account =>
            {
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

                if (@event.Email.HasValueTrimmed())
                {
                    account.Email = @event.Email;
                }
                    
                account.DefaultTipPercent = @event.DefaultTipPercent;

                account.Settings = settings;
            });
        }

        public void Handle(RoleAddedToUserAccount @event)
        {
            _projections.Update(@event.SourceId, account =>
            {
                account.Roles |= (int)Enum.Parse(typeof(Roles), @event.RoleName);
            });
        }

        public void Handle(RoleUpdatedToUserAccount @event)
        {
            _projections.Update(@event.SourceId, account =>
            {
                account.Roles = (int)Enum.Parse(typeof(Roles), @event.RoleName);
            });
        }

        public void Handle(CreditCardAddedOrUpdated @event)
        {
            _projections.Update(@event.SourceId, account =>
            {
                if (!account.DefaultCreditCard.HasValue)
                {
                    account.DefaultCreditCard = @event.CreditCardId;
                    account.Settings.ChargeTypeId = ChargeTypes.CardOnFile.Id;
                }
            });
        }

        public void Handle(DefaultCreditCardUpdated @event)
        {
            _projections.Update(@event.SourceId, account =>
            {
                account.Settings.ChargeTypeId = ChargeTypes.CardOnFile.Id;
                account.DefaultCreditCard = @event.CreditCardId;
            });
        }

        public void Handle(CreditCardRemoved @event)
        {
            _projections.Update(@event.SourceId, account =>
            {
                // used for migration, if user removed one card but had another one, we set this one as the default card
                account.DefaultCreditCard = @event.NextDefaultCreditCardId;
                account.Settings.ChargeTypeId = @event.NextDefaultCreditCardId.HasValue ? ChargeTypes.CardOnFile.Id : ChargeTypes.PaymentInCar.Id;
            });
            
        }

        public void Handle(AllCreditCardsRemoved @event)
        {
            _projections.Update(@event.SourceId, account =>
            {
                account.DefaultCreditCard = null;

                account.Settings.ChargeTypeId = account.IsPayPalAccountLinked
                    ? ChargeTypes.PayPal.Id
                    : ChargeTypes.PaymentInCar.Id;

            });
        }

        public void Handle(CreditCardDeactivated @event)
        {
            if (@event.IsOutOfAppPaymentDisabled == OutOfAppPaymentDisabled.None)
            {
                _projections.Update(@event.SourceId, account =>
                {
                    account.Settings.ChargeTypeId = ChargeTypes.PaymentInCar.Id;
                });
            }
        }

        public void Handle(AccountLinkedToIbs @event)
        {
            _projections.Update(@event.SourceId, account =>
            {
                if (!@event.CompanyKey.HasValue())
                {
                    account.IBSAccountId = @event.IbsAccountId;
                }
            });
        }

        public void Handle(AccountUnlinkedFromIbs @event)
        {
            _projections.Update(@event.SourceId, account =>
            {
                account.IBSAccountId = null;
            });
        }

        public void Handle(PayPalAccountLinked @event)
        {
            _projections.Update(@event.SourceId, account =>
            {
                account.IsPayPalAccountLinked = true;
                account.Settings.ChargeTypeId = ChargeTypes.PayPal.Id;
            });
        }

        public void Handle(PayPalAccountUnlinked @event)
        {
            _projections.Update(@event.SourceId, account =>
            {
                account.IsPayPalAccountLinked = false;
            });
        }

        public void Handle(ChargeAccountPaymentDisabled @event)
        {
            _projections.Update(HasChargeAccount, account =>
            {
                account.Settings.CustomerNumber = null;
                account.Settings.AccountNumber = null;
            });
        }

        private bool HasChargeAccount(AccountDetail accountDetail)
        {
            return accountDetail.Settings.AccountNumber.HasValue() ||
                   accountDetail.Settings.CustomerNumber.HasValue();
        }

        public void Handle(OverduePaymentSettled @event)
        {
            if (@event.IsPayInTaxiEnabled.HasValue && @event.IsPayInTaxiEnabled.Value)
            {
                _projections.Update(@event.SourceId, account =>
                {
                    //Re-enable card on file as the default payment method
                    account.Settings.ChargeTypeId = ChargeTypes.CardOnFile.Id;
                });
            }
        }
    }
 
}