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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;

#endregion

namespace DatabaseInitializer.Services
{
    public class EventsPlayBackService : IEventsPlayBackService, IEventHandlerRegistry
    {
        private readonly Func<EventStoreDbContext> _contextFactory;
        private readonly ITextSerializer _serializer;
        private readonly EventMigrator _migrator;
        private readonly ILogger _logger;
        private List<string> _eventsHandled = new List<string>();
        readonly EventDispatcher _eventDispatcher = new EventDispatcher();

        public EventsPlayBackService(Func<EventStoreDbContext> contextFactory,
            ITextSerializer serializer, EventMigrator migrator, ILogger logger)
        {
            _contextFactory = contextFactory;
            _serializer = serializer;
            _migrator = migrator;
            _logger = logger;
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
            const int pageSize = 10000;
           
            _logger.LogMessage("Replaying event since {0}", after);
            
            int eCount = 0;
            int migratedEventCount = 0;
            int actuallyReplayedEventCount = 0;
            var stopWatchPLayingEvents = new Stopwatch();

            stopWatchPLayingEvents.Start();
            using (var context = _contextFactory.Invoke())
            {
                foreach (var @event in GetConsumingEnumerable(after))
                {
                    eCount++;
                    if(!_eventsHandled.Contains(@event.EventType))
                    {
                        continue;
                    }

                    try
                    {
                        var ev = Deserialize(@event);

                        ////migration
                        var migratedEvent = _migrator.MigrateEvent(ev);
                        ////TODO find a way to be more subtil to detect if an event needs to be migrated
                        if (migratedEvent != null)
                        {
                            migratedEventCount++;
                            ev = (IVersionedEvent)migratedEvent;
                            //@event.Payload = _serializer.Serialize(migratedEvent);
                            //TODO should we save it in another table ?
                            //context.Set<Event>().Attach(@event);
                            //context.Entry(@event).State = EntityState.Modified;
                            //context.Entry(@event).Property(u => u.Payload).IsModified = true;

                            if ((migratedEventCount % 1000) == 0)
                            {
                                //context.SaveChanges();
                            }
                        }

                        actuallyReplayedEventCount++;
                        _eventDispatcher.DispatchMessage(ev);

                        if((eCount % 10000) == 0)
                        {
                            _logger.LogMessage("{0} events played", eCount);
                            _logger.LogMessage("{0} events actually replayed", actuallyReplayedEventCount);
                        }
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
                context.SaveChanges();
               
            }

            _logger.LogMessage("{0} events played", eCount);
            _logger.LogMessage("{0} events actually replayed", actuallyReplayedEventCount);
            stopWatchPLayingEvents.Stop();
            _logger.LogMessage("Replayed events in " + stopWatchPLayingEvents.Elapsed.TotalMinutes + " minutes");
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

        public void Register(IEventHandler handler)
        {
            _eventDispatcher.Register(handler);

            var eventNames = handler.GetType().GetInterfaces()
                               .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                               .Select(i => i.GetGenericArguments()[0])
                               .Select(i => i.FullName)
                               .ToList();

            foreach(var name in eventNames)
            {
                if(!_eventsHandled.Contains(name))
                {
                    _eventsHandled.Add(name);
                }
            }
        }

        private IEnumerable<Event> GetConsumingEnumerable(DateTime? after)
        {
            var result = new BlockingCollection<Event>();
            var pageSize = 100000;
            var skip = 0;

            Task.Factory.StartNew(() =>
            {
                var stopWatchGettingsEvents = new Stopwatch();
                double gettingEventSeconds = 0;

                using (var context = _contextFactory.Invoke())
                {
                    context.Database.CommandTimeout = 0;
                    bool hasMore = false;
                    do
                    {
                        while(result.Count > 10000)
                        {
                            _logger.LogMessage("{0} events in queue, sleeping for a bit...", result.Count);
                            Thread.Sleep(1000);
                        }
                        hasMore = false;
                        _logger.LogMessage("Getting next events...");
                        stopWatchGettingsEvents.Restart();

                        // order by date then by version in case two events happened at the same time
                        var events = context.Set<Event>().AsNoTracking()
                                    //.Where(x => new[] { "Order", "Company", "Account" }.Contains(x.AggregateType) )
                                    .OrderBy(x => x.EventDate)
                                    .ThenBy(x => x.Version)
                                    .Skip(skip)
                                    .Take(pageSize);

                        if (after.HasValue)
                        {
                            events = events.Where(x => x.EventDate > after);
                        }

                        var sql = ((System.Data.Entity.Infrastructure.DbQuery<Event>)events).ToString();
                        _logger.LogMessage(sql);

                        foreach (var ev in events)
                        {
                            result.Add(ev);
                            hasMore = true;
                        }

                        skip += pageSize;
                        _logger.LogMessage("Done getting next events in " + stopWatchGettingsEvents.Elapsed.TotalSeconds + " seconds");
                        gettingEventSeconds += stopWatchGettingsEvents.Elapsed.TotalSeconds;
                    }
                    while (hasMore);

                    _logger.LogMessage("Done getting events in " + gettingEventSeconds + " seconds");

                    result.CompleteAdding();
                }
            }, TaskCreationOptions.LongRunning);


            return result.GetConsumingEnumerable();

        }
    }
}