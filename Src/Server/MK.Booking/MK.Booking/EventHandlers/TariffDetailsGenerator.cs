﻿using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.EventHandlers
{
    public class TariffDetailsGenerator: IEventHandler<TariffCreated>, IEventHandler<TariffUpdated>, IEventHandler<TariffDeleted>
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
                    FlatRate = @event.FlatRate,
                    KilometricRate = @event.KilometricRate,
                    MarginOfError = @event.MarginOfError,
                    PassengerRate = @event.PassengerRate,
                    DaysOfTheWeek = (int)@event.DaysOfTheWeek,
                    StartTime =  @event.StartTime < (DateTime)SqlDateTime.MinValue ? (DateTime)SqlDateTime.MinValue:  @event.StartTime,
                    EndTime = @event.EndTime < (DateTime)SqlDateTime.MinValue ? (DateTime)SqlDateTime.MinValue : @event.EndTime,
                    Type = (int)@event.Type
                });   
            }
        }

        public void Handle(TariffUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var tariff = context.Find<TariffDetail>(@event.TariffId);
                tariff.Name = @event.Name;
                tariff.FlatRate = @event.FlatRate;
                tariff.KilometricRate = @event.KilometricRate;
                tariff.MarginOfError = @event.MarginOfError;
                tariff.PassengerRate = @event.PassengerRate;
                tariff.DaysOfTheWeek = (int) @event.DaysOfTheWeek;
                tariff.StartTime = @event.StartTime < (DateTime)SqlDateTime.MinValue
                                ? (DateTime) SqlDateTime.MinValue
                                : @event.StartTime;
                tariff.EndTime = @event.EndTime < (DateTime)SqlDateTime.MinValue
                              ? (DateTime) SqlDateTime.MinValue
                              : @event.EndTime;

                context.SaveChanges();
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
    }
}
