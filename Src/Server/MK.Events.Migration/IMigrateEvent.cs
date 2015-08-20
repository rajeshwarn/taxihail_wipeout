using Infrastructure.Messaging;

namespace apcurium.MK.Events.Migration
{
    public interface IMigrateEvent<TEvent> where TEvent : IEvent
    {
        TEvent Migrate(TEvent @event);
    }
}