using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Events.Migration.Processor
{
    /// <summary>
    /// Processes incoming events from the bus and routes them to the appropriate 
    /// handlers.
    /// </summary>
    public class EventProcessor
    {
        private readonly EventDispatcher _messageDispatcher;

        public EventProcessor()
        {
            this._messageDispatcher = new EventDispatcher();
        }

        public void Register(IMigrateEvent eventHandler)
        {
            this._messageDispatcher.Register(eventHandler);
        }

        public IEvent ProcessMessage(IEvent  @event)
        {
            return this._messageDispatcher.DispatchMessage(@event);
        }
    }
}
