using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using DeviceCollectorBindingsAndroid;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;

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

            var topActivity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();

            GenerateSessionId();

            var deviceCollector = new DeviceCollector(topActivity.Activity);
            deviceCollector.SetCollectorUrl(DeviceCollectorUrl());
            deviceCollector.SetMerchantId(MerchantId()); 
            deviceCollector.Collect(GetSessionId());

            #endif
        }
    }
}