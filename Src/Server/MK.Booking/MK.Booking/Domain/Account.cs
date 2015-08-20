#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.EventSourcing;
using MK.Common.Configuration;

#endregion

namespace apcurium.MK.Booking.Domain
{
    public class Account : EventSourced
    {
        private readonly IList<Guid> _favoriteAddresses = new List<Guid>();
        private string _confirmationToken;

        protected Account(Guid id) : base(id)
        {
            Handles<AccountRegistered>(OnAccountRegistered);
            Handles<AccountConfirmed>(NoAction);
            Handles<AccountDisabled>(NoAction);
            Handles<AccountUpdated>(NoAction);
            Handles<FavoriteAddressAdded>(OnAddressAdded);
            Handles<FavoriteAddressRemoved>(OnAddressRemoved);
            Handles<FavoriteAddressUpdated>(OnAddressUpdated);
            Handles<AccountPasswordReset>(NoAction);
            Handles<BookingSettingsUpdated>(NoAction);
            Handles<AccountPasswordUpdated>(NoAction);
            Handles<AddressRemovedFromHistory>(NoAction);
            Handles<RoleAddedToUserAccount>(NoAction);
            Handles<CreditCardAddedOrUpdated>(NoAction);
            Handles<CreditCardRemoved>(NoAction);
            Handles<AllCreditCardsRemoved>(NoAction);
            Handles<DeviceRegisteredForPushNotifications>(NoAction);
            Handles<DeviceUnregisteredForPushNotifications>(NoAction);
            Handles<NotificationSettingsAddedOrUpdated>(NoAction);
            Handles<UserTaxiHailNetworkSettingsAddedOrUpdated>(NoAction);
            Handles<AccountLinkedToIbs>(NoAction);
            Handles<AccountUnlinkedFromIbs>(NoAction);
            Handles<PayPalAccountLinked>(NoAction);
            Handles<PayPalAccountUnlinked>(NoAction);
            Handles<CreditCardDeactivated>(NoAction);
            Handles<OverduePaymentLogged>(NoAction);
            Handles<OverduePaymentSettled>(NoAction);
            Handles<AccountAnswersAddedUpdated>(NoAction);
        }

        public Account(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {
            LoadFrom(history);
        }

        public Account(Guid id, string name, CountryISOCode country, string phone, string email, byte[] password, 
            string confirmationToken, string language, bool accountActivationDisabled, string payBack, int nbPassenger, bool isAdmin = false)
            : this(id)
        {
            if (Params.Get(name, country.SelectOrDefault(countryCode => countryCode.Code), phone, email, confirmationToken).Any(p => p.IsNullOrEmpty())
                || (password == null))
            {
                throw new InvalidOperationException("Missing required fields");
            }
            Update(new AccountRegistered
            {
                SourceId = id,
                Name = name,
                Email = email,
                Country = country,
                Phone = phone,
                Password = password,
                ConfirmationToken = confirmationToken,
                Language = language,
                IsAdmin = isAdmin,
                AccountActivationDisabled = accountActivationDisabled,
                PayBack = payBack,
                NbPassengers = nbPassenger
            });
        }

        public Account(Guid id, string name, CountryISOCode country, string phone, string email, string payBack, int nbPassenger, string facebookId = null,
            string twitterId = null, string language = null, bool isAdmin = false)
            : this(id)
        {
            if (Params.Get(name, country.Code, phone, email).Any(p => p.IsNullOrEmpty()))
            {
                throw new InvalidOperationException("Missing required fields");
            }
            Update(new AccountRegistered
            {
                SourceId = id,
                Name = name,
                Email = email,
                Country = country,
                Phone = phone,
                TwitterId = twitterId,
                FacebookId = facebookId,
                Language = language,
                IsAdmin = isAdmin,
                PayBack = payBack,
                NbPassengers = nbPassenger
            });
        }

        public void ConfirmAccount(string confirmationToken)
        {
            if (confirmationToken == null) throw new ArgumentNullException();
            if (confirmationToken.Length == 0)
                throw new ArgumentException("Confirmation token cannot be an empty string");
            if (confirmationToken != _confirmationToken)
                throw new InvalidOperationException("Invalid confirmation token");

            Update(new AccountConfirmed());
        }

        internal void Update(string name)
        {
            if (Params.Get(name).Any(p => p.IsNullOrEmpty()))
            {
                throw new InvalidOperationException("Missing required fields");
            }

            Update(new AccountUpdated
            {
                SourceId = Id,
                Name = name,
            });
        }

        internal void ResetPassword(byte[] newPassword)
        {
            if (Params.Get(newPassword).Any(p => false))
            {
                throw new InvalidOperationException("Missing required fields");
            }

            Update(new AccountPasswordReset
            {
                SourceId = Id,
                Password = newPassword
            });
        }

        internal void UpdatePassword(byte[] newPassword)
        {
            if (Params.Get(newPassword).Any(p => false))
            {
                throw new InvalidOperationException("Missing required fields");
            }

            Update(new AccountPasswordUpdated
            {
                SourceId = Id,
                Password = newPassword
            });
        }

        public void UpdateBookingSettings(BookingSettings settings, int? defaultTipPercent)
        {
            Update(new BookingSettingsUpdated
            {
                SourceId = Id,
                Name = settings.Name,
                ChargeTypeId = settings.ChargeTypeId,
                NumberOfTaxi = settings.NumberOfTaxi,
                Passengers = settings.Passengers,
                Country = settings.Country,
                Phone = settings.Phone,
                ProviderId = settings.ProviderId,
                VehicleTypeId = settings.VehicleTypeId,
                AccountNumber = settings.AccountNumber,
                CustomerNumber = settings.CustomerNumber,
                DefaultTipPercent = defaultTipPercent,
                PayBack = settings.PayBack
            });
        }

        public void AddFavoriteAddress(Address address)
        {
            ValidateFavoriteAddress(address.FriendlyName, address.FullAddress, address.Latitude, address.Longitude);

            Update(new FavoriteAddressAdded
            {
                Address = address
            });
        }

        public void UpdateFavoriteAddress(Address address)
        {
            ValidateFavoriteAddress(address.FriendlyName, address.FullAddress, address.Latitude, address.Longitude);

            Update(new FavoriteAddressUpdated
            {
                Address = address
            });
        }

        public void RemoveFavoriteAddress(Guid addressId)
        {
            if (!_favoriteAddresses.Contains(addressId))
            {
                throw new InvalidOperationException("Address does not exist in account");
            }

            Update(new FavoriteAddressRemoved
            {
                AddressId = addressId
            });
        }

        public void RemoveAddressFromHistory(Guid addressId)
        {
            Update(new AddressRemovedFromHistory {AddressId = addressId});
        }

        public void AddOrUpdateCreditCard(string creditCardCompany, Guid creditCardId, string nameOnCard, 
            string last4Digits, string expirationMonth, string expirationYear, string token)
        {
            Update(new CreditCardAddedOrUpdated
            {
                CreditCardCompany = creditCardCompany,
                CreditCardId = creditCardId,
                NameOnCard = nameOnCard,
                Last4Digits = last4Digits,
                ExpirationMonth = expirationMonth,
                ExpirationYear = expirationYear,
                Token = token
            });
        }

        public void RemoveAllCreditCards()
        {
            Update(new AllCreditCardsRemoved());
        }

        public void AddRole(string rolename)
        {
            Update(new RoleAddedToUserAccount
            {
                RoleName = rolename,
            });
        }

        public void RegisterDeviceForPushNotifications(string deviceToken, PushNotificationServicePlatform platform)
        {
            if (Params.Get(deviceToken).Any(p => p.IsNullOrEmpty()))
            {
                throw new InvalidOperationException("Missing device token");
            }

            Update(new DeviceRegisteredForPushNotifications
            {
                DeviceToken = deviceToken,
                Platform = platform,
            });
        }

        public void UnregisterDeviceForPushNotifications(string deviceToken)
        {
            if (Params.Get(deviceToken).Any(p => p.IsNullOrEmpty()))
            {
                throw new InvalidOperationException("Missing device token");
            }

            Update(new DeviceUnregisteredForPushNotifications
            {
                DeviceToken = deviceToken,
            });
        }

        private void OnAccountRegistered(AccountRegistered @event)
        {
            _confirmationToken = @event.ConfirmationToken;
        }

        private void OnAddressAdded(FavoriteAddressAdded @event)
        {
            _favoriteAddresses.Add(@event.Address.Id);
        }

        private void OnAddressRemoved(FavoriteAddressRemoved @event)
        {
            _favoriteAddresses.Remove(@event.AddressId);
        }

        private void OnAddressUpdated(FavoriteAddressUpdated @event)
        {
            if (!_favoriteAddresses.Contains(@event.Address.Id))
            {
                _favoriteAddresses.Add(@event.Address.Id);
            }
        }

        private static void ValidateFavoriteAddress(string friendlyName, string fullAddress, double latitude,
            double longitude)
        {
            if (Params.Get(friendlyName, fullAddress).Any(p => p.IsNullOrEmpty()))
            {
                throw new InvalidOperationException("Missing required fields");
            }

            if (latitude < -90 || latitude > 90)
            {
                throw new ArgumentOutOfRangeException("latitude", "Invalid latitude");
            }

            if (longitude < -180 || latitude > 180)
            {
                throw new ArgumentOutOfRangeException("longitude", "Invalid longitude");
            }
        }

        public void EnableAccountByAdmin()
        {
            Update(new AccountConfirmed());
        }

        public void DisableAccountByAdmin()
        {
            Update(new AccountDisabled());
        }

        public void AddOrUpdateNotificationSettings(NotificationSettings notificationSettings)
        {
            notificationSettings.Id = Id;

            Update(new NotificationSettingsAddedOrUpdated
            {
                NotificationSettings = notificationSettings
            });
        }

        public void AddOrUpdateTaxiHailNetworkSettings(bool isEnabled, string[] disabledFleets)
        {
            Update(new UserTaxiHailNetworkSettingsAddedOrUpdated
            {
                IsEnabled = isEnabled,
                DisabledFleets = disabledFleets
            });
        }

        public void LinkToIbs(string companyKey, int ibsAccountId)
        {
            Update(new AccountLinkedToIbs
            {
                CompanyKey = companyKey,
                IbsAccountId = ibsAccountId
            });
        }

        public void UnlinkFromIbs()
        {
            Update(new AccountUnlinkedFromIbs());
        }

        public void LinkPayPalAccount(string encryptedRefreshToken)
        {
            Update(new PayPalAccountLinked{ EncryptedRefreshToken = encryptedRefreshToken });
        }

        public void UnlinkPayPalAccount()
        {
            Update(new PayPalAccountUnlinked());
        }

        public void ReactToPaymentFailure(Guid orderId, int? ibsOrderId, decimal amount, string transactionId, DateTime? transactionDate, FeeTypes feeType, bool isOutOfAppPaymentDisabled)
        {
            Update(new CreditCardDeactivated
            {
                IsOutOfAppPaymentDisabled = isOutOfAppPaymentDisabled
            });
            Update(new OverduePaymentLogged
            {
                OrderId = orderId,
                IBSOrderId = ibsOrderId,
                Amount = amount,
                TransactionId = transactionId,
                TransactionDate = transactionDate,
                FeeType = feeType
            });
        }

        public void SettleOverduePayment(Guid orderId, bool isPayInTaxiEnabled)
        {
            Update(new OverduePaymentSettled
            {
                OrderId = orderId,
                IsPayInTaxiEnabled = isPayInTaxiEnabled
            });
        }

        public void SaveQuestionAnswers(IEnumerable<AccountChargeQuestionAnswer> answers)
        {
            Update(new AccountAnswersAddedUpdated
            {
                Answers = answers.ToArray()
            });
        }
    }
}