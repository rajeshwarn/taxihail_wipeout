using System;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.EventHandlers
{
    public class AccountDetailsGenerator :
        IEventHandler<AccountRegistered>,
        IEventHandler<AccountConfirmed>,
        IEventHandler<AccountUpdated>,
        IEventHandler<BookingSettingsUpdated>,
        IEventHandler<AccountPasswordReset>,
        IEventHandler<AccountPasswordUpdated>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private IConfigurationManager _configurationManager;
        public AccountDetailsGenerator(Func<BookingDbContext> contextFactory, IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
            _contextFactory = contextFactory;
        }

        public void Handle(AccountRegistered @event)
        {
            using (var context = _contextFactory.Invoke())
            {

                var account = new AccountDetail
                                 {
                                     Name = @event.Name,
                                     Email = @event.Email,
                                     Password = @event.Password,
                                     Phone = @event.Phone,
                                     Id = @event.SourceId,
                                     IBSAccountId = @event.IbsAcccountId,
                                     FacebookId = @event.FacebookId,
                                     TwitterId = @event.TwitterId
                                 };


                var chargeTypeId = int.Parse(_configurationManager.GetSetting("DefaultBookingSettings.ChargeTypeId"));
                var nbPassenger = int.Parse(_configurationManager.GetSetting("DefaultBookingSettings.NbPassenger"));
                var vehicleTypeId= int.Parse(_configurationManager.GetSetting("DefaultBookingSettings.VehicleTypeId"));
                var providerId= int.Parse(_configurationManager.GetSetting("DefaultBookingSettings.ProviderId"));

                account.Settings = new BookingSettingsDetails { ChargeTypeId = chargeTypeId, Name = account.Name, NumberOfTaxi = 1, Passengers = nbPassenger, Phone = account.Phone, ProviderId = providerId, VehicleTypeId = vehicleTypeId };

                context.Save(account);

            }
        }

        public void Handle(AccountConfirmed @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.IsConfirmed = true;

                context.Save(account);
            }
        }

        public void Handle(AccountUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.Name = @event.Name;

                context.Save(account);
            }
        }

        public void Handle(BookingSettingsUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                var settings = account.Settings ?? new BookingSettingsDetails();
                settings.Name = @event.Name;
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

        public void Handle(AccountPasswordReset @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var account = context.Find<AccountDetail>(@event.SourceId);
                account.Password = @event.Password;
                context.Save(account);
            }
        }
        public void Handle(AccountPasswordUpdated @event)
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
