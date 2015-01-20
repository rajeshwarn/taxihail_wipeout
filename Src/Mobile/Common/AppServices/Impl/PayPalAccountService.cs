using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class PayPalAccountService : BaseService, IPayPalAccountService
    {
        public Task LinkAccount(string authCode)
        {
            return UseServiceClientAsync<PayPalAccountServiceClient>(service => service.LinkAccount(authCode));
        }

        public Task UnLinkAccount()
        {
            return UseServiceClientAsync<PayPalAccountServiceClient>(service => service.UnLinkAccount());
        }
    }
}