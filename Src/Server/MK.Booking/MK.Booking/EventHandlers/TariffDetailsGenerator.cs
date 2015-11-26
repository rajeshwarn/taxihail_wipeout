using System;
using System.Data.SqlTypes;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Booking.ReadModel;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class TariffDetailsGenerator : 
        IEventHandler<TariffCreated>, 
        IEventHandler<TariffUpdated>,
        IEventHandler<TariffDeleted>
    {
        private readonly IProjectionSet<TariffDetail> _tariffProjectionSet;

        public TariffDetailsGenerator(IProjectionSet<TariffDetail> tariffProjectionSet)
        {
            _tariffProjectionSet = tariffProjectionSet;
        }

        public void Handle(TariffCreated @event)
        {
            _tariffProjectionSet.Add(new TariffDetail
            {
                CompanyId = @event.SourceId,
                Id = @event.TariffId,
                Name = @event.Name,
                MinimumRate = @event.MinimumRate,
                FlatRate = @event.FlatRate,
                KilometricRate = @event.KilometricRate,
                PerMinuteRate = @event.PerMinuteRate,
                MarginOfError = @event.MarginOfError,
                KilometerIncluded = @event.KilometerIncluded,
                DaysOfTheWeek = (int)@event.DaysOfTheWeek,
                StartTime = SqlDateTimeMinValueSafeGuard(@event.StartTime),
                EndTime = SqlDateTimeMinValueSafeGuard(@event.EndTime),
                Type = (int)@event.Type,
                VehicleTypeId = @event.VehicleTypeId
            });
        }

        public void Handle(TariffDeleted @event)
        {
            _tariffProjectionSet.Remove(@event.TariffId);
        }

        public void Handle(TariffUpdated @event)
        {
            _tariffProjectionSet.Update(@event.TariffId, tariff =>
            {
                tariff.Name = @event.Name;
                tariff.MinimumRate = @event.MinimumRate;
                tariff.FlatRate = @event.FlatRate;
                tariff.KilometricRate = @event.KilometricRate;
                tariff.PerMinuteRate = @event.PerMinuteRate;
                tariff.MarginOfError = @event.MarginOfError;
                tariff.KilometerIncluded = @event.KilometerIncluded;
                tariff.DaysOfTheWeek = (int)@event.DaysOfTheWeek;
                tariff.StartTime = SqlDateTimeMinValueSafeGuard(@event.StartTime);
                tariff.EndTime = SqlDateTimeMinValueSafeGuard(@event.EndTime);
                tariff.VehicleTypeId = @event.VehicleTypeId;
            });
        }

        private DateTime SqlDateTimeMinValueSafeGuard(DateTime value)
        {
            return value < (DateTime) SqlDateTime.MinValue
                ? (DateTime) SqlDateTime.MinValue
                : value;
        }
    }
}