using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using Braintree;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class DeviceCollectorService : BaseDeviceCollectorService
    {
        private DeviceCollectorSDK _deviceCollector;

        public DeviceCollectorService(IAppSettings settings)
            : base(settings)
        {
        }

        public override void GenerateNewSessionIdAndCollect()
        {
            #if !DEBUG

            GenerateSessionId();

            _deviceCollector = new DeviceCollectorSDK();
            _deviceCollector.SetCollectorUrl(DeviceCollectorUrl());
            _deviceCollector.SetMerchantId(MerchantId()); 
            _deviceCollector.Collect(GetSessionId());

            #endif
        }
    }
}
