#region

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Diagnostic;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Infrastructure.Sql.EventSourcing;
using ServiceStack.Text;
using apcurium.MK.Events.Migration;
using Infrastructure.Messaging.Handling;

#endregion

namespace DatabaseInitializer.Services
{
    public class EventsPlayBackService : IEventsPlayBackService
    {
        private readonly Func<EventStoreDbContext> _contextFactory;
        private readonly IEventBus _eventBus;
        private readonly ITextSerializer _serializer;
        private readonly EventMigrator _migrator;
        private readonly ILogger _logger;
        private List<Type> _eventsHandled;

        public EventsPlayBackService(Func<EventStoreDbContext> contextFactory, IEventBus eventBus,
            ITextSerializer serializer, EventMigrator migrator, ILogger logger)
        {
            _contextFactory = contextFactory;
            _eventBus = eventBus;
            _serializer = serializer;
            _migrator = migrator;
            _logger = logger;

            _eventsHandled = typeof(AccountDetailsGenerator).GetInterfaces()
                                           .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                                           .Select(i => i.GetGenericArguments()[0])
                                           .ToList();
        }

        public int CountEvent(string aggregateType)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Set<Event>().Count(ev => ev.AggregateType == aggregateType);
            }
        }

    

        public void ReplayAllEvents(DateTime? after = null)
        {
            var skip = 0;
            var hasMore = true;
            const int pageSize = 100000;
           
            _logger.LogMessage("Replaying event since {0}", after);
            
            int eCount = 0;
            int migratedEventCount = 0;
            int actuallyReplayedEventCount = 0;
            var stopWatchGettingsEvents = new Stopwatch();
            var stopWatchPLayingEvents = new Stopwatch();
            double gettingEventSeconds = 0;

            stopWatchPLayingEvents.Start();
            while (hasMore)
            {
                List<Event> events;

                _logger.LogMessage("Getting next events...");
                stopWatchGettingsEvents.Start();
                using (var context = _contextFactory.Invoke())
                {
                    context.Database.CommandTimeout = 0;
                    // order by date then by version in case two events happened at the same time
                    var result = context.Set<Event>()
                                    .OrderBy(x => x.EventDate)
                                    .ThenBy(x => x.Version)
                                    .Skip(skip)
                                    .Take(pageSize);

                    if (after != null)
                    {
                        result = result.Where(x => x.EventDate > after);
                    }

                    var sql = ((System.Data.Entity.Infrastructure.DbQuery<Event>)result).ToString();

                    _logger.LogMessage(sql);

                    events= result.ToList();
                }
                _logger.LogMessage("Done getting next events in " + stopWatchGettingsEvents.Elapsed.TotalSeconds + " seconds");
                gettingEventSeconds += stopWatchGettingsEvents.Elapsed.TotalSeconds;
                stopWatchGettingsEvents.Reset();
                

                if (events.Count == 0)
                {
                    _logger.LogMessage("No event to be replayed");
                    return;
                }
                
                hasMore = events.Count == pageSize;

                skip += pageSize;

                if (events.Any())
                {
                    foreach (var @event in events)
                    {
                        try
                        {
                            var ev = Deserialize(@event);

                            //migration
                            //var migratedEvent = _migrator.MigrateEvent(ev);
                            ////TODO find a way to be more subtil to detect if an event needs to be migrated
                            //if (migratedEvent != null)
                            //{
                            //    migratedEventCount++;
                            //    ev = (IVersionedEvent) migratedEvent;
                            //    @event.Payload = _serializer.Serialize(migratedEvent);
                            //    using (var context = _contextFactory.Invoke())
                            //    {
                            //        //TODO should we save it in another table ?
                            //        context.Set<Event>().Attach(@event);
                            //        context.Entry(@event).State = EntityState.Modified;
                            //        context.Entry(@event).Property(u => u.Payload).IsModified = true;
                            //        context.SaveChanges();
                            //    }
                            //}

                            //Replay only events for AccountDetails

                            if (_eventsHandled.Contains(ev.GetType()))
                            {
                                actuallyReplayedEventCount++;
                                _eventBus.Publish(new Envelope<IEvent>(ev)
                                {
                                    CorrelationId = @event.CorrelationId
                                });
                            }
                            eCount++;
                        }
                        catch
                        {
                            _logger.LogMessage("Error replaying an event : ");
                            if (@event != null)
                            {
                                _logger.LogMessage(@event.ToJson());
                            }
                            throw;
                        }
                    }
                    _logger.LogMessage("{0} events played", eCount);
                    //Console.WriteLine("{0} events migrated", migratedEventCount);
                    _logger.LogMessage("{0} events actually replayed", actuallyReplayedEventCount);
                    //Console.WriteLine("Number of events played: " + (hasMore ? skip : (skip + events.Count)));
                }
            }

            _logger.LogMessage("{0} events actually replayed", actuallyReplayedEventCount);
            stopWatchPLayingEvents.Stop();
            _logger.LogMessage("Replayed events in " + stopWatchPLayingEvents.Elapsed.TotalMinutes + " minutes");
            _logger.LogMessage("Including Getting events in " + gettingEventSeconds + " seconds");
        }

        private IVersionedEvent Deserialize(Event @event)
        {
            using (var reader = new StringReader(@event.Payload))
            {
                var e = (IVersionedEvent) _serializer.Deserialize(reader);
                if (e is IUpgradableEvent)
                {
                    e = ((IUpgradableEvent) e).Upgrade();
                }
                return e;
            }
        }
    }
}