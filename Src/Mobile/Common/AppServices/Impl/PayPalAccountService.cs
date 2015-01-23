using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Payments.PayPal;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.PayPal;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class PayPalAccountService : BaseService, IPayPalAccountService
    {
        public Task LinkAccount(Guid accoundId, string authCode)
        {
            return UseServiceClientAsync<PayPalServiceClient>(service => 
                service.LinkAccount(new LinkPayPalAccountRequest
                {
                    AccountId = accoundId,
                    AuthCode = authCode
                }));
        }

        public Task UnLinkAccount(Guid accoundId)
        {
            return UseServiceClientAsync<PayPalServiceClient>(service => 
                service.UnLinkAccount(new UnlinkPayPalAccountRequest
                {
                    AccountId = accoundId
                }));
        }
    }
}