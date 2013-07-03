using System;
using MK.Booking.Api.Client.TaxiHail;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class PayPalExpressCheckoutService: BaseService, IPayPalExpressCheckoutService
    {
        public Task<string> SetExpressCheckoutForAmount(decimal amount)
        {
            return Task.Factory.StartNew(()=>{
                string checkoutUrl = string.Empty;
                UseServiceClient<PayPalServiceClient> (client => {
                    checkoutUrl = client.SetExpressCheckoutForAmount(amount);
                });
                return checkoutUrl;
            });
        }
    }
}

