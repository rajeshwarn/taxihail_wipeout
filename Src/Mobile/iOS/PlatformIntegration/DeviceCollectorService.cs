using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using DeviceCollectorBindingsIOS;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class DeviceCollectorService : BaseDeviceCollectorService
    {
        public DeviceCollectorService(IAppSettings settings)
            : base(settings)
        {
        }

        public override string CollectAndReturnSessionId()
        {
            var sessionId = GenerateSessionId();

            var debugLogging = false;

            #if DEBUG
            debugLogging = true;
            #endif

            var deviceCollector = new DeviceCollectorSDK(debugLogging);
            deviceCollector.SetCollectorUrl(DeviceCollectorUrl());
            deviceCollector.SetMerchantId(MerchantId()); 
            deviceCollector.Collect(sessionId);

            return sessionId;
        }
    }
}
