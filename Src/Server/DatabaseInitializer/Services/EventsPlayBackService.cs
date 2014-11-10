#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Infrastructure.Sql.EventSourcing;

#endregion

namespace DatabaseInitializer.Services
{
    public class EventsPlayBackService : IEventsPlayBackService
    {
        private readonly Func<EventStoreDbContext> _contextFactory;
        private readonly IEventBus _eventBus;
        private readonly ITextSerializer _serializer;

        public EventsPlayBackService(Func<EventStoreDbContext> contextFactory, IEventBus eventBus,
            ITextSerializer serializer)
        {
            _contextFactory = contextFactory;
            _eventBus = eventBus;
            _serializer = serializer;
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
            const int pageSize = 5000;
            after = after ?? DateTime.MinValue;

            Console.WriteLine("Replaying event since {0}", after);

            while(hasMore)
            {
                List<Event> events;
                using (var context = _contextFactory.Invoke())
                {
                    context.Database.CommandTimeout = 2000;
                    // order by date then by version in case two events happened at the same time
                    events = context.Set<Event>()
                                    .OrderBy(x => x.EventDate)
                                    .ThenBy(x => x.Version)
                                    .Where(x => x.EventDate > after)
                                    .Skip(skip)
                                    .Take(pageSize)
                                    .ToList();
                }

                if (events.Count == 0)
                {
                    Console.WriteLine("No event to be replayed");
                    return;
                }
                
                hasMore = events.Count == pageSize;
                Console.WriteLine("Number of events played: " + (hasMore ? skip : (skip + events.Count)));
                skip += pageSize;
                
                if (events.Any())
                {
                    foreach (var @event in events)
                    {
                        _eventBus.Publish(new Envelope<IEvent>(Deserialize(@event))
                        {
                            CorrelationId = @event.CorrelationId
                        });
                    }
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