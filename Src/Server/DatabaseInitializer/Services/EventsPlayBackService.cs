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

        public void ReplayAllEvents()
        {
            IEnumerable<Event> allEvents;
            using (var context = _contextFactory.Invoke())
            {
                allEvents = context.Set<Event>().OrderBy(x => x.EventDate).ToList();
            }

            if (allEvents.Any())
            {
                foreach (var @event in allEvents)
                {
                    _eventBus.Publish(new Envelope<IEvent>(Deserialize(@event)) {CorrelationId = @event.CorrelationId});
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