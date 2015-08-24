using apcurium.MK.Events.Migration.Processor;
using Infrastructure.Messaging;


namespace apcurium.MK.Events.Migration
{
    public class EventMigrator
    {
        private readonly EventProcessor _eventProcessor;

        public EventMigrator(EventProcessor @eventProcessor)
        {
            _eventProcessor = eventProcessor;
        }

        public IEvent MigrateEvent(IEvent @event)
        {
            return _eventProcessor.ProcessMessage(@event);
        }

    }
}