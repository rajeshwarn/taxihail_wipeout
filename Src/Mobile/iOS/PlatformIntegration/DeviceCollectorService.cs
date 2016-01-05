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

            var deviceCollector = new DeviceCollectorSDK(false);
            deviceCollector.SetCollectorUrl(DeviceCollectorUrl);
            deviceCollector.SetMerchantId(MerchantId); 
            deviceCollector.Collect(sessionId);

            return sessionId;
        }
    }
}
