using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using DeviceCollectorBindingsAndroid;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class DeviceCollectorService : BaseDeviceCollectorService
    {
        public override string CollectAndReturnSessionId()
        {     
            var topActivity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();

            var sessionId = GenerateSessionId();

            var deviceCollector = new DeviceCollector(topActivity.Activity);
            deviceCollector.SetCollectorUrl(DeviceCollectorUrl);
            deviceCollector.SetMerchantId(MerchantId); 
            deviceCollector.Collect(sessionId);

            return sessionId;
        }
    }
}