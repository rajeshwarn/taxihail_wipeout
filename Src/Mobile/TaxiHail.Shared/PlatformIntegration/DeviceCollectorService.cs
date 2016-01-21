using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using Braintree.Device_Collector;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class DeviceCollectorService : BaseDeviceCollectorService
    {
        private DeviceCollector _deviceCollector;

        public DeviceCollectorService(IAppSettings settings)
            : base(settings)
        {
        }

        public override void GenerateNewSessionIdAndCollect()
        {     
            #if !DEBUG

            var topActivity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();

            GenerateSessionId();

            _deviceCollector = new DeviceCollector(topActivity.Activity);
            _deviceCollector.SetCollectorUrl(DeviceCollectorUrl());
            _deviceCollector.SetMerchantId(MerchantId()); 
            _deviceCollector.Collect(GetSessionId());

            #endif
        }
    }
}