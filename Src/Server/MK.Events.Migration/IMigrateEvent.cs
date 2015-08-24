using Infrastructure.Messaging;

namespace apcurium.MK.Events.Migration
{
    public interface IMigrateEvent
    {
    }

    public interface IMigrateEvent<TEvent> : IMigrateEvent where TEvent : IEvent
    {
        TEvent Migrate(TEvent @event);
    }
}