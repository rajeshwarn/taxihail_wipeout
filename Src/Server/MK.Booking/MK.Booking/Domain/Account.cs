using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.EventSourcing;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
namespace apcurium.MK.Booking.Domain
{
    public class Account : EventSourced
    {
        private readonly IList<Guid> _favoriteAddresses = new List<Guid>();
        private string _confirmationToken;
        protected Account(Guid id) : base(id)
        {
            base.Handles<AccountRegistered>(OnAccountRegistered);
            base.Handles<AccountConfirmed>(NoAction);
            base.Handles<AccountUpdated>(NoAction);
            base.Handles<FavoriteAddressAdded>(OnAddressAdded);
            base.Handles<FavoriteAddressRemoved>(OnAddressRemoved);
            base.Handles<FavoriteAddressUpdated>(OnAddressUpdated);
            base.Handles<AccountPasswordReset>(NoAction);
            base.Handles<BookingSettingsUpdated>(NoAction);
            base.Handles<AccountPasswordUpdated>(NoAction);
            base.Handles<AddressRemovedFromHistory>(OnAddressRemoved);
            base.Handles<AdminRightGranted>(NoAction);
            base.Handles<CreditCardAdded>(OnCreditCardAdded);
            base.Handles<CreditCardRemoved>(OnCreditCardRemoved);
            base.Handles<PaymentProfileUpdated>(OnPaymentProfileUpdated);
        }


        public Account(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {               
            this.LoadFrom(history);
        }

        public Account(Guid id, string name,string phone, string email, byte[] password, int ibsAccountId, string confirmationToken, string language, bool isAdmin=false)
            : this(id)
        {
            if (Params.Get(name, phone, email, confirmationToken).Any(p => p.IsNullOrEmpty())
                || ibsAccountId == 0 || (password == null) )
            {
                throw new InvalidOperationException("Missing required fields");
            }
            this.Update(new AccountRegistered
            {
                SourceId = id,
                Name = name,
                Email = email,
                Phone = phone,
                Password = password,
                IbsAcccountId = ibsAccountId,
                ConfirmationToken = confirmationToken,
                Language = language,
                IsAdmin = isAdmin
            });
        }

        public Account(Guid id, string name, string phone, string email, int ibsAccountId, string facebookId = null, string twitterId = null, string language = null, bool isAdmin = false)
            : this(id)
        {
            if (Params.Get(name, phone, email).Any(p => p.IsNullOrEmpty())
                || ibsAccountId == 0 )
            {
                throw new InvalidOperationException("Missing required fields");
            }
            this.Update(new AccountRegistered
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
            if(confirmationToken.Length == 0) throw new ArgumentException("Confirmation token cannot be an empty string");
            if(confirmationToken != this._confirmationToken) throw new InvalidOperationException("Invalid confirmation token");

            this.Update(new AccountConfirmed());  
        }
        
        internal void Update( string name )
        {
            if (Params.Get(name).Any(p => p.IsNullOrEmpty()))
            {
                throw new InvalidOperationException("Missing required fields");
            }

            this.Update(new AccountUpdated
            {                 
                SourceId= Id,
                Name = name,                
            });        
        }

        internal void ResetPassword(byte[] newPassword)
        {
            if (Params.Get(newPassword).Any(p => false))
            {
                throw new InvalidOperationException("Missing required fields");
            }

            this.Update(new AccountPasswordReset()
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

            this.Update(new AccountPasswordUpdated()
            {
                SourceId = Id,
                Password = newPassword
            });
        }

        public void UpdateBookingSettings(BookingSettings settings)
        {
            this.Update(new BookingSettingsUpdated
            {
                SourceId = Id,
                Name = settings.Name,                
                ChargeTypeId = settings.ChargeTypeId,
                NumberOfTaxi = settings.NumberOfTaxi,
                Passengers = settings.Passengers,
                Phone = settings.Phone,
                ProviderId = settings.ProviderId,
                VehicleTypeId = settings.VehicleTypeId
            });  
        }

        public void AddFavoriteAddress(Address address)
        {
            ValidateFavoriteAddress(address.FriendlyName, address.FullAddress, address.Latitude, address.Longitude);

            this.Update(new FavoriteAddressAdded
            {
                Address = address
            });
        }

        public void UpdateFavoriteAddress(Address address)
        {
            ValidateFavoriteAddress(address.FriendlyName, address.FullAddress, address.Latitude, address.Longitude);

            this.Update(new FavoriteAddressUpdated()
            {
               Address = address
            });
        }

        public void RemoveFavoriteAddress(Guid addressId)
        {
            if(!_favoriteAddresses.Contains(addressId))
            {
                throw new InvalidOperationException("Address does not exist in account");
            }

            this.Update(new FavoriteAddressRemoved
            {
                AddressId = addressId
            });
        }

        public void RemoveAddressFromHistory(Guid addressId)
        {
            this.Update(new AddressRemovedFromHistory() { AddressId = addressId });
        }


        public void AddCreditCard(string creditCardCompany, Guid creditCardId, string friendlyName, string last4Digits, string token)
        {
            this.Update(new CreditCardAdded
            {
                CreditCardCompany = creditCardCompany,
                CreditCardId = creditCardId,
                FriendlyName = friendlyName,
                Last4Digits = last4Digits,
                Token = token
            });
        }

        public void RemoveCreditCard(Guid creditCardId)
        {
            this.Update(new CreditCardRemoved
            {
                CreditCardId = creditCardId
            });
        }

        public void UpdatePaymentProfile(Guid? defaultCreditCard, double? defaultTipAmount, double? defaultTipPercent)
        {
            this.Update(new PaymentProfileUpdated
            {
                DefaultCreditCard = defaultCreditCard,
                DefaultTipAmount = defaultTipAmount,
                DefaultTipPercent = defaultTipPercent
            });
        }

        public void GrantAdminRight()
        {
            this.Update(new AdminRightGranted());
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
        }

        private void OnCreditCardRemoved(CreditCardRemoved obj)
        {
        }

        private void OnPaymentProfileUpdated(PaymentProfileUpdated obj)
        {
            
        }


        private void NoAction<T>(T @event) where T : VersionedEvent
        {
            
        }



        private static void ValidateFavoriteAddress(string friendlyName, string fullAddress, double latitude, double longitude)
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


    }
}
