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
        protected Account(Guid id) : base(id)
        {
            base.Handles<AccountRegistered>(OnAccountRegistered);
            base.Handles<AccountUpdated>(OnAccountUpdated);
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


        private void OnAccountRegistered(AccountRegistered @event)
        {

        }


        private void OnAccountUpdated(AccountUpdated @event)
        {

        }
    }
}
