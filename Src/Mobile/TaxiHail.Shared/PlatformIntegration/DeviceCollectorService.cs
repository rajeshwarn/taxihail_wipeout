using apcurium.MK.Booking.Mobile.Infrastructure;
using Braintree.Device_Collector;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Configuration.Impl;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class DeviceCollectorService : BaseDeviceCollectorService
    {
        private DeviceCollector _deviceCollector;

        public DeviceCollectorService(IPaymentService paymentService)
            : base(paymentService)
        {
        }

        public override async Task GenerateNewSessionIdAndCollect()
        {     
            #if !DEBUG

            var paymentSettings = await PaymentService.GetPaymentSettings();
            if(paymentSettings.PaymentMode == PaymentMethod.Cmt
                || paymentSettings.PaymentMode == PaymentMethod.RideLinqCmt)
            {
                // Kount is only enabled for CMT

                var topActivity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();

                GenerateSessionId();

                _deviceCollector = new DeviceCollector(topActivity.Activity);
                _deviceCollector.SetCollectorUrl(DeviceCollectorUrl(paymentSettings.CmtPaymentSettings.IsSandbox));
                _deviceCollector.SetMerchantId(MerchantId); 
                _deviceCollector.Collect(GetSessionId());
            }

            #endif
        }
    }
}