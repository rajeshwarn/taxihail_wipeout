using apcurium.MK.Booking.Mobile.Infrastructure;
using Braintree;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class DeviceCollectorService : BaseDeviceCollectorService
    {
        private DeviceCollectorSDK _deviceCollector;

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

                GenerateSessionId();

                _deviceCollector = new DeviceCollectorSDK();
                _deviceCollector.SetCollectorUrl(DeviceCollectorUrl(paymentSettings.CmtPaymentSettings.IsSandbox));
                _deviceCollector.SetMerchantId(MerchantId); 
                _deviceCollector.Collect(GetSessionId());
            }

            #endif
        }
    }
}
