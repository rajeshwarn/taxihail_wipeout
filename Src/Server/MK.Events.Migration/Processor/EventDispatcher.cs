using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Events;
using Infrastructure.Messaging;

namespace apcurium.MK.Events.Migration.Processor
{
    public class EventDispatcher
    {
        
        
        private Dictionary<Type, List<IMigrateEvent>> migratorsByEventType;    

        public EventDispatcher()
        {
            this.migratorsByEventType = new Dictionary<Type, List<IMigrateEvent>>();
        }

        public EventDispatcher(IEnumerable<IMigrateEvent> handlers)
            : this()
        {
            foreach (var handler in handlers)
            {
                this.Register(handler);
            }
        }

        public void Register(IMigrateEvent handler)
        {
            var interfaces = handler.GetType().GetInterfaces();

            var eventsHandled =
                interfaces
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMigrateEvent<>))
                    .Select(i => new { HandlerInterface = i, EventType = i.GetGenericArguments()[0] })
                    .Select(e => new Tuple<Type, IMigrateEvent>(e.EventType, handler))
                    .ToList();

            foreach (var eventHandled in eventsHandled)
            {
                List<IMigrateEvent> invocations;
                if (!this.migratorsByEventType.TryGetValue(eventHandled.Item1, out invocations))
                {
                    invocations = new List<IMigrateEvent>();
                    this.migratorsByEventType[eventHandled.Item1] = invocations;
                }
                invocations.Add(eventHandled.Item2);
            }
        }

        public IEvent DispatchMessage(IEvent @event)
        {
            var eventType = @event.GetType();
            IEvent migratedEvent = null;
            if (migratorsByEventType.ContainsKey(eventType))
            {
                var migrators = migratorsByEventType[eventType];
                foreach (dynamic migrator in migrators)
                {
                    //TODO : optmize this with caching of the type and method or using Lambda expression
                    var interfaceType = typeof (IMigrateEvent<>);
                    var genericInterfaceType = interfaceType.MakeGenericType(eventType);
                    var migrateMethod = genericInterfaceType.GetMethod("Migrate");
                    migratedEvent = migrateMethod.Invoke(migrator, new object[] {@event});
                }
            }

            return migratedEvent;
        }
    }
}