﻿#region

using System;
using System.Data.SqlTypes;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using Infrastructure.Messaging.Handling;

#endregion

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
            using (var context = _contextFactory.Invoke())
            {
                context.Save(new TariffDetail
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
                    DaysOfTheWeek = (int) @event.DaysOfTheWeek,
                    StartTime =
                        @event.StartTime < (DateTime) SqlDateTime.MinValue
                            ? (DateTime) SqlDateTime.MinValue
                            : @event.StartTime,
                    EndTime =
                        @event.EndTime < (DateTime) SqlDateTime.MinValue
                            ? (DateTime) SqlDateTime.MinValue
                            : @event.EndTime,
                    Type = (int) @event.Type,
                    VehicleTypeId = @event.VehicleTypeId,
                    ServiceType = @event.ServiceType
                });
            }
        }

        public void Handle(TariffDeleted @event)
        {
            using (var context = _contextFactory.Invoke())
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
            using (var context = _contextFactory.Invoke())
            {
                var tariff = context.Find<TariffDetail>(@event.TariffId);
                tariff.Name = @event.Name;
                tariff.MinimumRate = @event.MinimumRate;
                tariff.FlatRate = @event.FlatRate;
                tariff.KilometricRate = @event.KilometricRate;
                tariff.PerMinuteRate = @event.PerMinuteRate;
                tariff.MarginOfError = @event.MarginOfError;
                tariff.KilometerIncluded = @event.KilometerIncluded;
                tariff.DaysOfTheWeek = (int) @event.DaysOfTheWeek;
                tariff.StartTime = @event.StartTime < (DateTime) SqlDateTime.MinValue
                    ? (DateTime) SqlDateTime.MinValue
                    : @event.StartTime;
                tariff.EndTime = @event.EndTime < (DateTime) SqlDateTime.MinValue
                    ? (DateTime) SqlDateTime.MinValue
                    : @event.EndTime;
                tariff.VehicleTypeId = @event.VehicleTypeId;
                tariff.ServiceType = @event.ServiceType;
                context.SaveChanges();
            }
        }
    }
}