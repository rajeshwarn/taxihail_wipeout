using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class PayPalExpressCheckoutService: BaseService, IPayPalExpressCheckoutService
    {
        public Task<string> SetExpressCheckoutForAmount(Guid orderId, decimal amount, decimal meterAmout,decimal tipAmount)
        {
            var client = TinyIoC.TinyIoCContainer.Current.Resolve<PayPalServiceClient>();
            return client.SetExpressCheckoutForAmount(orderId, amount, meterAmout, tipAmount);
        }
    }
}

