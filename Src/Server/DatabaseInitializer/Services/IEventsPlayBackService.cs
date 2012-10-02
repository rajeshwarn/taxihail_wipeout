using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Infrastructure.Sql.EventSourcing;

namespace DatabaseInitializer.Services
{
    public interface IEventsPlayBackService
    {
        void ReplayAllEvents(); 
    }

    public class EventsPlayBackService  : IEventsPlayBackService
    {
        private readonly Func<EventStoreDbContext> _contextFactory;
        private readonly IEventBus _eventBus;
        private readonly ITextSerializer _serializer;

        public EventsPlayBackService(Func<EventStoreDbContext> contextFactory, IEventBus eventBus, ITextSerializer serializer)
        {
            _contextFactory = contextFactory;
            _eventBus = eventBus;
            _serializer = serializer;
        }

        public void ReplayAllEvents()
        {
            IEnumerable<Event> allEvents;
            using(var context =_contextFactory.Invoke())
            {
                allEvents = context.Set<Event>().ToList();
            }

            if(allEvents.Any())
            {
                foreach (var @event in allEvents)
                {
                    _eventBus.Publish(new Envelope<IEvent>(Deserialize(@event)) { CorrelationId = @event.CorrelationId });
                }
               
            }
        }

        private IVersionedEvent Deserialize(Event @event)
        {
            using (var reader = new StringReader(@event.Payload))
            {
                return (IVersionedEvent)_serializer.Deserialize(reader);
            }
        }
    }
}