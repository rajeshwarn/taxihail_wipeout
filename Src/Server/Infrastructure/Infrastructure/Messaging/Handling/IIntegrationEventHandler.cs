
namespace Infrastructure.Messaging.Handling
{
    /// <summary>
    /// Marker interface.
    /// Integration event handlers never replay events.
    /// Useful for events that send emails or a push notifications.
    /// </summary>
    public interface IIntegrationEventHandler : IEventHandler
    {
    }
}
