#region

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Infrastructure.Sql.EventSourcing;
using ServiceStack.Text;
using apcurium.MK.Events.Migration;

#endregion

namespace DatabaseInitializer.Services
{
    public class EventsPlayBackService : IEventsPlayBackService
    {
        private readonly Func<EventStoreDbContext> _contextFactory;
        private readonly IEventBus _eventBus;
        private readonly ITextSerializer _serializer;
        private readonly EventMigrator _migrator;

        public EventsPlayBackService(Func<EventStoreDbContext> contextFactory, IEventBus eventBus,
            ITextSerializer serializer, EventMigrator migrator)
        {
            _contextFactory = contextFactory;
            _eventBus = eventBus;
            _serializer = serializer;
            _migrator = migrator;
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
            after = after ?? DateTime.MinValue;
            
            Console.WriteLine("Replaying event since {0}", after);
            
            int eCount = 0;
            int migratedEventCount = 0;

            while (hasMore)
            {
                List<Event> events;
                

                Console.WriteLine("Getting next events...");
                using (var context = _contextFactory.Invoke())
                {
                    context.Database.CommandTimeout = 0;
                    // order by date then by version in case two events happened at the same time
                    events = context.Set<Event>()
                                    .OrderBy(x => x.EventDate)
                                    .ThenBy(x => x.Version)
                                    .Where(x => x.EventDate > after)
                                    .Skip(skip)
                                    .Take(pageSize)
                                    .ToList();
                }

                Console.WriteLine("Done getting next events...");

                if (events.Count == 0)
                {
                    Console.WriteLine("No event to be replayed");
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
                            var migratedEvent = _migrator.MigrateEvent(ev);
                            //TODO find a way to be more subtil to detect if an event needs to be migrated
                            if (migratedEvent != null)
                            {
                                migratedEventCount++;
                                ev = (IVersionedEvent) migratedEvent;
                                @event.Payload = _serializer.Serialize(migratedEvent);
                                using (var context = _contextFactory.Invoke())
                                {
                                    //TODO should we save it in another table ?
                                    context.Set<Event>().Attach(@event);
                                    context.Entry(@event).State = EntityState.Modified;
                                    context.Entry(@event).Property(u => u.Payload).IsModified = true;
                                    context.SaveChanges();
                                }
                            }

                            _eventBus.Publish(new Envelope<IEvent>(ev)
                            {
                                CorrelationId = @event.CorrelationId
                            });

                            eCount++;

                            if ( eCount % 5000 == 0)
                            {
                                Console.Write("{0} events played" , eCount);
                                Console.Write("{0} events migrated", migratedEventCount);
                            }                            
                        }
                        catch
                        {
                            Console.Write("Error replaying an event : ");
                            if (@event != null)
                            {                                
                                Console.Write(@event.ToJson());
                            }
                            throw;
                        }
                    }

                    Console.WriteLine("Number of events played: " + (hasMore ? skip : (skip + events.Count)));
                }
            }
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