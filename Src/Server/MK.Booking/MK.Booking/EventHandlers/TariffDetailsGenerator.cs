using System;
using System.Data.SqlTypes;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class TariffDetailsGenerator : IEventHandler<TariffCreated>, IEventHandler<TariffUpdated>,
        IEventHandler<TariffDeleted>
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public TariffDetailsGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(TariffCreated @event)
        {
            using (BookingDbContext context = _contextFactory.Invoke())
            {
                context.Save(new TariffDetail
                {
                    CompanyId = @event.SourceId,
                    Id = @event.TariffId,
                    Name = @event.Name,
                    FlatRate = @event.FlatRate,
                    KilometricRate = @event.KilometricRate,
                    MarginOfError = @event.MarginOfError,
                    KilometerIncluded = @event.KilometerIncluded,
                    PassengerRate = @event.PassengerRate,
                    DaysOfTheWeek = (int) @event.DaysOfTheWeek,
                    StartTime =
                        @event.StartTime < (DateTime) SqlDateTime.MinValue
                            ? (DateTime) SqlDateTime.MinValue
                            : @event.StartTime,
                    EndTime =
                        @event.EndTime < (DateTime) SqlDateTime.MinValue
                            ? (DateTime) SqlDateTime.MinValue
                            : @event.EndTime,
                    Type = (int) @event.Type
                });
            }
        }

        public void Handle(TariffDeleted @event)
        {
            using (BookingDbContext context = _contextFactory.Invoke())
            {
                var tariff = context.Find<TariffDetail>(@event.TariffId);
                if (tariff != null)
                {
                    context.Set<TariffDetail>().Remove(tariff);
                    context.SaveChanges();
                }
            }
        }

        public void Handle(TariffUpdated @event)
        {
            using (BookingDbContext context = _contextFactory.Invoke())
            {
                var tariff = context.Find<TariffDetail>(@event.TariffId);
                tariff.Name = @event.Name;
                tariff.FlatRate = @event.FlatRate;
                tariff.KilometricRate = @event.KilometricRate;
                tariff.MarginOfError = @event.MarginOfError;
                tariff.KilometerIncluded = @event.KilometerIncluded;
                tariff.PassengerRate = @event.PassengerRate;
                tariff.DaysOfTheWeek = (int) @event.DaysOfTheWeek;
                tariff.StartTime = @event.StartTime < (DateTime) SqlDateTime.MinValue
                    ? (DateTime) SqlDateTime.MinValue
                    : @event.StartTime;
                tariff.EndTime = @event.EndTime < (DateTime) SqlDateTime.MinValue
                    ? (DateTime) SqlDateTime.MinValue
                    : @event.EndTime;

                context.SaveChanges();
            }
        }
    }
}