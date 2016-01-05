using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using DeviceCollectorBindingsIOS;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class DeviceCollectorService : BaseDeviceCollectorService
    {
        public override string CollectAndReturnSessionId()
        {
            var sessionId = GenerateSessionId();

            var debugLogging = false;

            #if DEBUG
            debugLogging = true;
            #endif

            var deviceCollector = new DeviceCollectorSDK(debugLogging);
            deviceCollector.SetCollectorUrl(DeviceCollectorUrl);
            deviceCollector.SetMerchantId(MerchantId); 
            deviceCollector.Collect(sessionId);

            return sessionId;
        }
    }
}
