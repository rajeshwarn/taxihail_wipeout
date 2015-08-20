#region

using System;
using System.Reflection;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using AutoMapper;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.CommandHandlers
{
    public class AccountCommandHandler : ICommandHandler<RegisterAccount>,
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
        ICommandHandler<AddOrUpdateCreditCard>,
        ICommandHandler<DeleteAllCreditCards>,
        ICommandHandler<DeleteAccountCreditCards>,
        ICommandHandler<RegisterDeviceForPushNotifications>,
        ICommandHandler<UnregisterDeviceForPushNotifications>,
        ICommandHandler<AddFavoriteAddress>,
        ICommandHandler<RemoveFavoriteAddress>,
        ICommandHandler<UpdateFavoriteAddress>,
        ICommandHandler<RemoveAddressFromHistory>,
        ICommandHandler<LogApplicationStartUp>,
        ICommandHandler<LinkAccountToIbs>,
        ICommandHandler<AddOrUpdateUserTaxiHailNetworkSettings>,
        ICommandHandler<UnlinkAccountFromIbs>,
        ICommandHandler<LinkPayPalAccount>,
        ICommandHandler<UnlinkPayPalAccount>,
        ICommandHandler<UnlinkAllPayPalAccounts>,
        ICommandHandler<ReactToPaymentFailure>,
        ICommandHandler<SettleOverduePayment>,
        ICommandHandler<AddUpdateAccountQuestionAnswer>
    {
        private readonly IPasswordService _passwordService;
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly IServerSettings _serverSettings;
        private readonly IEventSourcedRepository<Account> _repository;

        public AccountCommandHandler(IEventSourcedRepository<Account> repository, IPasswordService passwordService, Func<BookingDbContext> contextFactory, IServerSettings _serverSettings)
        {
            _repository = repository;
            _passwordService = passwordService;
            _contextFactory = contextFactory;
            this._serverSettings = _serverSettings;
        }

        public void Handle(AddOrUpdateCreditCard command)
        {
            var account = _repository.Find(command.AccountId);
            account.AddOrUpdateCreditCard(
                command.CreditCardCompany,
                command.CreditCardId,
                command.NameOnCard,
                command.Last4Digits,
                command.ExpirationMonth,
                command.ExpirationYear,
                command.Token);
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(AddRoleToUserAccount command)
        {
            var account = _repository.Find(command.AccountId);
            account.AddRole(command.RoleName);
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(ConfirmAccount command)
        {
            var account = _repository.Find(command.AccountId);
            account.ConfirmAccount(command.ConfimationToken);
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(DisableAccountByAdmin command)
        {
            var account = _repository.Find(command.AccountId);
            account.DisableAccountByAdmin();
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(EnableAccountByAdmin command)
        {
            var account = _repository.Find(command.AccountId);
            account.EnableAccountByAdmin();
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(RegisterAccount command)
        {
            var password = _passwordService.EncodePassword(command.Password, command.AccountId.ToString());
            var account = new Account(command.AccountId, command.Name, command.Country, command.Phone, command.Email, password,
                command.ConfimationToken, command.Language, command.AccountActivationDisabled, command.PayBack, _serverSettings.ServerData.DefaultBookingSettings.NbPassenger, command.IsAdmin);
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

        public void Handle(RegisterFacebookAccount command)
        {
            var account = new Account(command.AccountId, command.Name, command.Country, command.Phone, command.Email,
                command.PayBack, _serverSettings.ServerData.DefaultBookingSettings.NbPassenger, command.FacebookId, language: command.Language);
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(RegisterTwitterAccount command)
        {
            var account = new Account(command.AccountId, command.Name, command.Country, command.Phone, command.Email,
                command.PayBack, _serverSettings.ServerData.DefaultBookingSettings.NbPassenger, twitterId: command.TwitterId, language: command.Language);
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(DeleteAccountCreditCards command)
        {
            var account = _repository.Find(command.AccountId);
            account.RemoveAllCreditCards();
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

        public void Handle(ResetAccountPassword command)
        {
            var account = _repository.Find(command.AccountId);
            var newPassword = _passwordService.EncodePassword(command.Password, command.AccountId.ToString());
            account.ResetPassword(newPassword);
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(UnregisterDeviceForPushNotifications command)
        {
            var account = _repository.Find(command.AccountId);
            account.UnregisterDeviceForPushNotifications(command.DeviceToken);
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(UpdateAccount command)
        {
            var account = _repository.Find(command.AccountId);
            account.Update(command.Name);
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(UpdateAccountPassword command)
        {
            var account = _repository.Find(command.AccountId);
            var newPassword = _passwordService.EncodePassword(command.Password, command.AccountId.ToString());
            account.UpdatePassword(newPassword);
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(UpdateBookingSettings command)
        {
            var account = _repository.Find(command.AccountId);

            var settings = new BookingSettings();
            Mapper.Map(command, settings);

            if (command.ChargeTypeId == _serverSettings.ServerData.DefaultBookingSettings.ChargeTypeId)
            {
                command.ChargeTypeId = null;
            }

            if (command.VehicleTypeId == _serverSettings.ServerData.DefaultBookingSettings.VehicleTypeId)
            {
                command.VehicleTypeId = null;
            }

            if (command.ProviderId == _serverSettings.ServerData.DefaultBookingSettings.ProviderId)
            {
                command.ProviderId = null;
            }

            account.UpdateBookingSettings(settings, command.DefaultTipPercent);

            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(AddOrUpdateUserTaxiHailNetworkSettings command)
        {
            var account = _repository.Get(command.AccountId);
            account.AddOrUpdateTaxiHailNetworkSettings(command.IsEnabled, command.DisabledFleets);
            _repository.Save(account, command.Id.ToString());
        }

        #region Addresses

        public void Handle(AddFavoriteAddress command)
        {
            var account = _repository.Get(command.AccountId);
            account.AddFavoriteAddress(command.Address);
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(RemoveAddressFromHistory command)
        {
            var account = _repository.Get(command.AccountId);
            account.RemoveAddressFromHistory(command.AddressId);
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

        #endregion

        public void Handle(LogApplicationStartUp command)
        {
            using (var context = _contextFactory.Invoke())
            {
                // Check if a log from this user already exists. If not, create it.
                var log = context.Find<AppStartUpLogDetail>(command.UserId) ?? new AppStartUpLogDetail
                {
                    UserId = command.UserId,
                };

                // Update log details
                log.DateOccured = command.DateOccured;
                log.ApplicationVersion = command.ApplicationVersion;
                log.Platform = command.Platform;
                log.PlatformDetails = command.PlatformDetails;
                log.ServerVersion = Assembly.GetAssembly(typeof (AppStartUpLogDetail)).GetName().Version.ToString();
                log.Latitude = command.Latitude;
                log.Longitude = command.Longitude;

                context.Save(log);
            }
        }

        public void Handle(LinkAccountToIbs command)
        {
            var account = _repository.Find(command.AccountId);

            account.LinkToIbs(command.CompanyKey, command.IbsAccountId);

            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(UnlinkAccountFromIbs command)
        {
            var account = _repository.Find(command.AccountId);

            account.UnlinkFromIbs();

            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(LinkPayPalAccount command)
        {
            var account = _repository.Find(command.AccountId);

            account.LinkPayPalAccount(command.EncryptedRefreshToken);

            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(UnlinkPayPalAccount command)
        {
            var account = _repository.Find(command.AccountId);

            account.UnlinkPayPalAccount();

            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(UnlinkAllPayPalAccounts command)
        {
            foreach (var accountId in command.AccountIds)
            {
                var account = _repository.Find(accountId);
                account.UnlinkPayPalAccount();
                _repository.Save(account, command.Id.ToString());
            }
        }

        public void Handle(ReactToPaymentFailure command)
        {
            var account = _repository.Find(command.AccountId);

            account.ReactToPaymentFailure(command.OrderId, command.IBSOrderId, command.OverdueAmount, command.TransactionId, 
                command.TransactionDate, command.FeeType,
                _serverSettings.GetPaymentSettings().IsOutOfAppPaymentDisabled);

            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(SettleOverduePayment command)
        {
            var account = _repository.Find(command.AccountId);

            account.SettleOverduePayment(command.OrderId, _serverSettings.GetPaymentSettings().IsPayInTaxiEnabled);

            _repository.Save(account, command.Id.ToString());
        }
        public void Handle(AddUpdateAccountQuestionAnswer command)
        {
            var account = _repository.Find(command.AccountId);
            account.SaveQuestionAnswers(command.Answers);
            _repository.Save(account, command.Id.ToString());
        }
    }
}