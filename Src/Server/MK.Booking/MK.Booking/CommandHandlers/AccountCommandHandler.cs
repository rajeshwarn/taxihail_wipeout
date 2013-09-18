﻿using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using Infrastructure.EventSourcing;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.CommandHandlers
{
    public partial class AccountCommandHandler : ICommandHandler<RegisterAccount>,
                                                 ICommandHandler<ConfirmAccount>,
                                                 ICommandHandler<EnableAccountByAdmin>,
                                                 ICommandHandler<DisableAccountByAdmin>,
                                                 ICommandHandler<ResetAccountPassword>,
                                                 ICommandHandler<UpdateAccount>,
                                                 ICommandHandler<UpdateBookingSettings>,
                                                 ICommandHandler<RegisterFacebookAccount>,
                                                 ICommandHandler<RegisterTwitterAccount>,
                                                 ICommandHandler<UpdateAccountPassword>,
                                                 ICommandHandler<AddRoleToUserAccount>,
                                                 ICommandHandler<AddCreditCard>,
                                                 ICommandHandler<RemoveCreditCard>,
                                                 ICommandHandler<DeleteAllCreditCards>,
                                                 ICommandHandler<RegisterDeviceForPushNotifications>,
                                                 ICommandHandler<UnregisterDeviceForPushNotifications>,
                                                 ICommandHandler<AddFavoriteAddress>,
                                                 ICommandHandler<RemoveFavoriteAddress>,
                                                 ICommandHandler<UpdateFavoriteAddress>,
                                                 ICommandHandler<RemoveAddressFromHistory>
    {
        private readonly IEventSourcedRepository<Account> _repository;
        private readonly IPasswordService _passwordService;

        public AccountCommandHandler(IEventSourcedRepository<Account> repository, IPasswordService passwordService)
        {
            _repository = repository;
            _passwordService = passwordService;
        }

        public void Handle(RegisterAccount command)
        {
            var password = _passwordService.EncodePassword(command.Password, command.AccountId.ToString());
            var account = new Account(command.AccountId, command.Name, command.Phone, command.Email, password, command.IbsAccountId, command.ConfimationToken, command.Language, command.AccountActivationDisabled, command.IsAdmin);
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(ConfirmAccount command)
        {
            var account = _repository.Find(command.AccountId);
            account.ConfirmAccount(command.ConfimationToken);
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(UpdateAccount command)
        {
            var account = _repository.Find(command.AccountId);
            account.Update(command.Name);
            _repository.Save(account, command.Id.ToString());
            
        }

        public void Handle(ResetAccountPassword command)
        {
            var account = _repository.Find(command.AccountId);
            var newPassword = _passwordService.EncodePassword(command.Password, command.AccountId.ToString());
            account.ResetPassword(newPassword);
            _repository.Save(account, command.Id.ToString());
        }
        
        public void Handle(UpdateBookingSettings command)
        {
            var account = _repository.Find(command.AccountId);

            var settings = new BookingSettings();
            AutoMapper.Mapper.Map(command, settings);

            account.UpdateBookingSettings(settings);
            account.UpdatePaymentProfile(command.DefaultCreditCard, command.DefaultTipPercent);

            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(RegisterFacebookAccount command)
        {
            var account = new Account(command.AccountId, command.Name, command.Phone, command.Email, command.IbsAccountId, facebookId:command.FacebookId, language:command.Language);
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(RegisterTwitterAccount command)
        {
            var account = new Account(command.AccountId, command.Name, command.Phone, command.Email, command.IbsAccountId, twitterId: command.TwitterId, language: command.Language);
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(UpdateAccountPassword command)
        {
            var account = _repository.Find(command.AccountId);
            var newPassword = _passwordService.EncodePassword(command.Password, command.AccountId.ToString());
            account.UpdatePassword(newPassword);
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(AddRoleToUserAccount command)
        {
            var account = _repository.Find(command.AccountId);
            account.AddRole(command.RoleName);
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(AddCreditCard command)
        {
            var account = _repository.Find(command.AccountId);
            account.AddCreditCard(command.CreditCardCompany, command.CreditCardId, command.FriendlyName, command.Last4Digits, command.Token);
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(RemoveCreditCard command)
        {
            var account = _repository.Find(command.AccountId);
            account.RemoveCreditCard(command.CreditCardId);
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(RegisterDeviceForPushNotifications command)
        {
            var account = _repository.Find(command.AccountId);

            if (!string.IsNullOrEmpty(command.OldDeviceToken))
            {
                account.UnregisterDeviceForPushNotifications(command.OldDeviceToken);
            }

            account.RegisterDeviceForPushNotifications(command.DeviceToken, command.Platform);
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(UnregisterDeviceForPushNotifications command)
        {
            var account = _repository.Find(command.AccountId);
            account.UnregisterDeviceForPushNotifications(command.DeviceToken);
            _repository.Save(account, command.Id.ToString());
        }
        
        public void Handle(EnableAccountByAdmin command)
        {
            var account = _repository.Find(command.AccountId);
            account.EnableAccountByAdmin();
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(DisableAccountByAdmin command)
        {
            var account = _repository.Find(command.AccountId);
            account.DisableAccountByAdmin();
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(DeleteAllCreditCards command)
        {
            foreach (var accountId in command.AccountIds)
            {
                var account = _repository.Find(accountId);
            
                account.RemoveAllCreditCards();
                _repository.Save(account, command.Id.ToString());
            }
        }

        #region Addresses
        public void Handle(AddFavoriteAddress command)
        {
            var account = _repository.Get(command.AccountId);

            account.AddFavoriteAddress(command.Address);

            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(RemoveFavoriteAddress command)
        {
            var account = _repository.Get(command.AccountId);

            account.RemoveFavoriteAddress(command.AddressId);

            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(UpdateFavoriteAddress command)
        {
            var account = _repository.Get(command.AccountId);

            account.UpdateFavoriteAddress(command.Address);

            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(RemoveAddressFromHistory command)
        {
            var account = _repository.Get(command.AccountId);
            account.RemoveAddressFromHistory(command.AddressId);
            _repository.Save(account, command.Id.ToString());
        }
        #endregion
    }
}
