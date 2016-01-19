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

        public override void GenerateNewSessionIdAndCollect()
        {
            #if !DEBUG

            GenerateSessionId();

            var deviceCollector = new DeviceCollectorSDK();
            deviceCollector.SetCollectorUrl(DeviceCollectorUrl());
            deviceCollector.SetMerchantId(MerchantId()); 
            deviceCollector.Collect(GetSessionId());

            #endif
        }
    }
}
