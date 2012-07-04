using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.EventSourcing;
using apcurium.MK.Booking.Events;
using MoveOn.Common;
using MoveOn.Common.Extensions;

namespace apcurium.MK.Booking.Domain
{
    public class Account : EventSourced
    {
        protected Account(Guid id) : base(id)
        {
            base.Handles<AccountRegistered>(OnAccountRegistered);
            base.Handles<AccountUpdated>(OnAccountUpdated);
            base.Handles<FavoriteAddressAdded>(OnFavoriteAddressAdded);
        }

        public Account(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {               
            this.LoadFrom(history);
        }

        public Account(Guid id, string firstName, string lastName, string email, string password)
            : this(id)
        {
            if (Params.Get(firstName, lastName, email, password).Any(p => p.IsNullOrEmpty()))
            {
                throw new InvalidOperationException("Missing required fields");
            }
            this.Update(new AccountRegistered
            {
                SourceId = id,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
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


        private void OnAccountRegistered(AccountRegistered @event)
        {

        }


        private void OnAccountUpdated(AccountUpdated @event)
        {

        }

        private void OnFavoriteAddressAdded(FavoriteAddressAdded @event)
        {
            
        }

    }
}
