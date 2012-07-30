using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.EventSourcing;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
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
            base.Handles<AccountConfirmed>(OnAccountConfirmed);
            base.Handles<AccountUpdated>(OnAccountUpdated);
            base.Handles<AddressAdded>(OnAddressAdded);
            base.Handles<AddressRemoved>(OnAddressRemoved);
            base.Handles<AddressUpdated>(OnAddressUpdated);
            base.Handles<AccountPasswordReset>(OnAccountPasswordReset);
            base.Handles<BookingSettingsUpdated>(OnBookingSettingsUpdated);
            base.Handles<AccountPasswordUpdated>(OnAccountPasswordUpdated);
        }

        public Account(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {               
            this.LoadFrom(history);
        }

        public Account(Guid id, string name,string phone, string email, byte[] password, int ibsAccountId, string confirmationToken, string language)
            : this(id)
        {
            if (Params.Get(name,phone,email).Any(p => p.IsNullOrEmpty())
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
                Language = language
            });
        }

        public Account(Guid id, string name, string phone, string email, int ibsAccountId, string facebookId = null, string twitterId = null, string language = null)
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
                Language = language
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

        public void AddAddress(Guid id, string friendlyName, string apartment, string fullAddress, string ringCode, double latitude, double longitude, bool isHistoric)
        {
            ValidateFavoriteAddress(friendlyName, fullAddress, latitude, longitude);

            this.Update(new AddressAdded
            {
                AddressId = id,
                FriendlyName = friendlyName,
                Apartment = apartment,
                FullAddress = fullAddress,
                RingCode = ringCode,
                Latitude = latitude,
                Longitude = longitude,
                IsHistoric = isHistoric
            });
        }

        public void UpdateAddress(Guid id, string friendlyName, string apartment, string fullAddress, string ringCode, double latitude, double longitude, bool isHistoric)
        {
            ValidateFavoriteAddress(friendlyName, fullAddress, latitude, longitude);

            this.Update(new AddressUpdated()
            {
                AddressId = id,
                FriendlyName = friendlyName,
                Apartment = apartment,
                FullAddress = fullAddress,
                RingCode = ringCode,
                Latitude = latitude,
                Longitude = longitude,
                IsHistoric = isHistoric
            });
        }

        public void RemoveFavoriteAddress(Guid addressId)
        {
            if(!_favoriteAddresses.Contains(addressId))
            {
                throw new InvalidOperationException("Address does not exist in account");
            }

            this.Update(new AddressRemoved
            {
                AddressId = addressId
            });
        }

        private void OnAccountRegistered(AccountRegistered @event)
        {
            _confirmationToken = @event.ConfirmationToken;
        }

        private void OnAccountConfirmed(AccountConfirmed @event)
        {

        }

        private void OnAccountUpdated(AccountUpdated @event)
        {

        }

        private void OnAddressAdded(AddressAdded @event)
        {
            _favoriteAddresses.Add(@event.AddressId);
        }

        private void OnAddressRemoved(AddressRemoved @event)
        {
            _favoriteAddresses.Remove(@event.AddressId);
        }

        private void OnAddressUpdated(AddressUpdated obj)
        {

        }

        private void OnAccountPasswordReset(AccountPasswordReset obj)
        {

        }
        
        private void OnBookingSettingsUpdated(BookingSettingsUpdated obj)
        {
        }

        private void OnAccountPasswordUpdated(AccountPasswordUpdated obj)
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
