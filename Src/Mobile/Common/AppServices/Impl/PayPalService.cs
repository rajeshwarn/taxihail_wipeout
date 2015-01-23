using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Payments.PayPal;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.PayPal;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class PayPalService : BaseService, IPayPalService
    {
        public Task LinkAccount(Guid accoundId, string authCode)
        {
            return UseServiceClientAsync<PayPalServiceClient>(service => 
                service.LinkPayPalAccount(new LinkPayPalAccountRequest
                {
                    AccountId = accoundId,
                    AuthCode = authCode,
                    MetadataId = "test"
                }));
        }

        public Task UnLinkAccount(Guid accoundId)
        {
            return UseServiceClientAsync<PayPalServiceClient>(service =>
                service.UnlinkPayPalAccount(new UnlinkPayPalAccountRequest
                {
                    AccountId = accoundId
                }));
        }
    }
}