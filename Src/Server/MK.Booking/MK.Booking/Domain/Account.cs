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
        protected Account(Guid id) : base(id)
        {
            base.Handles<AccountRegistered>(OnAccountRegistered);
            base.Handles<AccountUpdated>(OnAccountUpdated);
            base.Handles<FavoriteAddressAdded>(OnFavoriteAddressAdded);
            base.Handles<FavoriteAddressRemoved>(OnFavoriteAddressRemoved);
        }

        public Account(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {               
            this.LoadFrom(history);
        }

        public Account(Guid id, string firstName, string lastName, string phone, string email, string password)
            : this(id)
        {
            if (Params.Get(firstName, lastName, phone,email, password).Any(p => p.IsNullOrEmpty()))
            {
                throw new InvalidOperationException("Missing required fields");
            }
            this.Update(new AccountRegistered
            {
                SourceId = id,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                Password = password,
            });
        }        
        
        internal void Update( string firstName, string lastName )
        {
            if (Params.Get(firstName, lastName).Any(p => p.IsNullOrEmpty()))
            {
                throw new InvalidOperationException("Missing required fields");
            }

            this.Update(new AccountUpdated
            {                 
                SourceId= Id,
                FirstName = firstName,
                LastName = lastName,
            });        
        }

        public void AddFavoriteAddress(string friendlyName, string apartment, string fullAddress, string ringCode, double latitude, double longitude)
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

            this.Update(new FavoriteAddressAdded
            {
                FriendlyName = friendlyName,
                Apartment = apartment,
                FullAddress = fullAddress,
                RingCode = ringCode,
                Latitude = latitude,
                Longitude = longitude
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


        private void OnAccountRegistered(AccountRegistered @event)
        {

        }


        private void OnAccountUpdated(AccountUpdated @event)
        {

        }

        private void OnFavoriteAddressAdded(FavoriteAddressAdded @event)
        {
            _favoriteAddresses.Add(@event.AddressId);
        }

        private void OnFavoriteAddressRemoved(FavoriteAddressRemoved @event)
        {
            _favoriteAddresses.Remove(@event.AddressId);
        }

        
    }
}
