#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
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
        private int _creditCardCount;
        private int? _defaultTipPercent;

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
            Handles<CreditCardAdded>(OnCreditCardAdded);
            Handles<CreditCardUpdated>(OnCreditCardUpdated);
            Handles<CreditCardRemoved>(OnCreditCardRemoved);
            Handles<AllCreditCardsRemoved>(OnAllCreditCardsRemoved);
            Handles<PaymentProfileUpdated>(OnPaymentProfileUpdated);
            Handles<DeviceRegisteredForPushNotifications>(NoAction);
            Handles<DeviceUnregisteredForPushNotifications>(NoAction);
            Handles<NotificationSettingsAddedOrUpdated>(NoAction);
        }

        public Account(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {
            LoadFrom(history);
        }

        public Account(Guid id, string name, string phone, string email, byte[] password, int ibsAccountId,
            string confirmationToken, string language, bool accountActivationDisabled, bool isAdmin = false)
            : this(id)
        {
            if (Params.Get(name, phone, email, confirmationToken).Any(p => p.IsNullOrEmpty())
                || ibsAccountId == 0 || (password == null))
            {
                throw new InvalidOperationException("Missing required fields");
            }
            Update(new AccountRegistered
            {
                SourceId = id,
                Name = name,
                Email = email,
                Phone = phone,
                Password = password,
                IbsAcccountId = ibsAccountId,
                ConfirmationToken = confirmationToken,
                Language = language,
                IsAdmin = isAdmin,
                AccountActivationDisabled = accountActivationDisabled
            });
        }

        public Account(Guid id, string name, string phone, string email, int ibsAccountId, string facebookId = null,
            string twitterId = null, string language = null, bool isAdmin = false)
            : this(id)
        {
            if (Params.Get(name, phone, email).Any(p => p.IsNullOrEmpty())
                || ibsAccountId == 0)
            {
                throw new InvalidOperationException("Missing required fields");
            }
            Update(new AccountRegistered
            {
                SourceId = id,
                Name = name,
                Email = email,
                Phone = phone,
                IbsAcccountId = ibsAccountId,
                TwitterId = twitterId,
                FacebookId = facebookId,
                Language = language,
                IsAdmin = isAdmin
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

        public void UpdateBookingSettings(BookingSettings settings)
        {
            Update(new BookingSettingsUpdated
            {
                SourceId = Id,
                Name = settings.Name,
                ChargeTypeId = settings.ChargeTypeId,
                NumberOfTaxi = settings.NumberOfTaxi,
                Passengers = settings.Passengers,
                Phone = settings.Phone,
                ProviderId = settings.ProviderId,
                VehicleTypeId = settings.VehicleTypeId,
                AccountNumber = settings.AccountNumber
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

        public void AddCreditCard(string creditCardCompany, Guid creditCardId, string nameOnCard, 
            string last4Digits, string expirationMonth, string expirationYear, string token)
        {
            Update(new CreditCardAdded
            {
                CreditCardCompany = creditCardCompany,
                CreditCardId = creditCardId,
                NameOnCard = nameOnCard,
                Last4Digits = last4Digits,
                ExpirationMonth = expirationMonth,
                ExpirationYear = expirationYear,
                Token = token
            });

            // Automatically set first credit card as default
            if (_creditCardCount == 1)
            {
                Update(new PaymentProfileUpdated
                {
                    DefaultCreditCard = creditCardId,
                    DefaultTipPercent = _defaultTipPercent
                });
            }
        }

        public void UpdateCreditCard(string creditCardCompany, Guid creditCardId, string nameOnCard,
            string last4Digits, string expirationMonth, string expirationYear, string token)
        {
            Update(new CreditCardUpdated
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

        public void RemoveCreditCard(Guid creditCardId)
        {
            Update(new CreditCardRemoved
            {
                CreditCardId = creditCardId
            });
        }

        public void RemoveAllCreditCards()
        {
            Update(new AllCreditCardsRemoved());
        }

        private void OnAllCreditCardsRemoved(AllCreditCardsRemoved obj)
        {
            _creditCardCount = 0;
        }

        public void UpdatePaymentProfile(Guid? defaultCreditCard, int? defaultTipPercent)
        {
            Update(new PaymentProfileUpdated
            {
                DefaultCreditCard = defaultCreditCard,
                DefaultTipPercent = defaultTipPercent
            });
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

        private void OnCreditCardAdded(CreditCardAdded obj)
        {
            _creditCardCount++;
        }

        private void OnCreditCardUpdated(CreditCardUpdated obj)
        {

        }

        private void OnCreditCardRemoved(CreditCardRemoved obj)
        {
            _creditCardCount = Math.Max(0, _creditCardCount - 1);
        }

        private void OnPaymentProfileUpdated(PaymentProfileUpdated @event)
        {
            _defaultTipPercent = @event.DefaultTipPercent;
        }

        private void NoAction<T>(T @event) where T : VersionedEvent
        {
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
// ReSharper disable LocalizableElement
                throw new ArgumentOutOfRangeException("latitude", "Invalid latitude");
            }

            if (longitude < -180 || latitude > 180)
            {
                throw new ArgumentOutOfRangeException("longitude", "Invalid longitude");
            }
// ReSharper restore LocalizableElement
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
    }
}