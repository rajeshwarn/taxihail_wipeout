using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;
using ServiceStack.CacheAccess;

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class CacheServiceManager : IEventHandler<AllCreditCardsRemoved>
    {
        private const string UserAuthIdPattern = "\"userAuthId\":\"{0}\"";

        private readonly ICacheClient _cacheService;

        public CacheServiceManager(ICacheClient cacheService)
        {
            _cacheService = cacheService;
        }

        public void Handle(AllCreditCardsRemoved @event)
        {
            if (!@event.ForceUserDisconnect)
            {
                return;
            }

            RemoveByPatternIfPossible(UserAuthIdPattern.InvariantCultureFormat(@event.SourceId));
        }

        private void RemoveByPatternIfPossible(string pattern)
        {
            var service = _cacheService as IRemoveByPattern;

            if (service == null)
            {
                return;
            }


            service.RemoveByPattern(pattern);
        }

    }
}