using System;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.EventHandlers
{
    public class AccountDetailsGenerator : IEventHandler<AccountRegistered>, IEventHandler<AccountUpdated>
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public AccountDetailsGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(AccountRegistered @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                
                context.Save(new AccountDetail
                {
                    FirstName = @event.FirstName,
                    LastName = @event.LastName,
                    Email = @event.Email,
                    Password = @event.Password,
                    Phone = @event.Phone,
                    Id = @event.SourceId,
                    IBSAccountid = @event.IbsAcccountId
                });

            }

        }

        public void Handle(AccountUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.FirstName = @event.FirstName;
                account.LastName = @event.LastName;

                context.Save(account);
            }
        }
    }
}
