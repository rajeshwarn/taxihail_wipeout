using System;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.EventHandlers
{
    public class AccountDetailsGenerator :
        IEventHandler<AccountRegistered>,
        IEventHandler<AccountUpdated>,
        IEventHandler<BookingSettingsUpdated>
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

        public void Handle(BookingSettingsUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                var settings = account.Settings ?? new BookingSettingsDetails();
                settings.FirstName = @event.FirstName;
                settings.LastName = @event.LastName;
                settings.ChargeTypeId = @event.ChargeTypeId;
                settings.NumberOfTaxi = @event.NumberOfTaxi;
                settings.Passengers = @event.Passengers;
                settings.Phone = @event.Phone;
                settings.ProviderId = @event.ProviderId;
                settings.VehicleTypeId = @event.VehicleTypeId;

                account.Settings = settings;
                context.Save(account);
            }
        }

        public void Handle(AccountPasswordResetted @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.Password = @event.Password;
                context.Save(account);
            }
        }
        
    }
}
