using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.PayPal;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Payment;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api
{
    public class ProcessPaymentController : BaseApiController
    {
        public ProcessPaymentService ProcessPaymentService { get; private set; }

        public ProcessPaymentController(IPayPalServiceFactory payPalServiceFactory,
            IPaymentService paymentService,
            IAccountDao accountDao,
            IOrderDao orderDao,
            IIBSServiceProvider ibsServiceProvider,
            IServerSettings serverSettings)
        {
            ProcessPaymentService = new ProcessPaymentService(payPalServiceFactory, paymentService, accountDao, orderDao, ibsServiceProvider, serverSettings);
        }

        [HttpPost, Auth, Route("api/v2/paypal/link")]
        public IHttpActionResult LinkPaypal(LinkPayPalAccountRequest request)
        {
            var result = ProcessPaymentService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Auth, Route("api/v2/paypal/unlink")]
        public IHttpActionResult UnlinkPaypal(UnlinkPayPalAccountRequest request)
        {
            var result = ProcessPaymentService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpDelete, Auth, Route("api/v2/payments/deleteToken/{cardToken}")]
        public IHttpActionResult DeleteCreditCard(string cardToken)
        {
            var result = ProcessPaymentService.Delete(cardToken);

            return GenerateActionResult(result);
        }

        [HttpPost, Auth, Route("api/v2/payments/unpair")]
        public async Task<IHttpActionResult> UnpairingForPayment(UnpairingForPaymentRequest request)
        {
            var result = await ProcessPaymentService.Post(request);

            return GenerateActionResult(result);
        }
    }
}
