using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Payment;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api
{
    [RoutePrefix("api/v2/order/pairing")]
    public class CmtPaymentPairingController : BaseApiController
    {
        public CmtPaymentPairingService CmtPaymentService { get; }

        public CmtPaymentPairingController(IOrderDao orderDao, IAccountDao accountDao, ICreditCardDao creditCardDao, ILogger logger, ICommandBus commandBus, IServerSettings serverSettings)
        {
            CmtPaymentService = new CmtPaymentPairingService(orderDao, accountDao, creditCardDao, logger, commandBus, serverSettings);
        }

        [HttpPost]
        public async Task<IHttpActionResult> Pair(CmtPaymentPairingRequest request)
        {
            var result = await CmtPaymentService.Post(request);

            return GenerateActionResult(result);
        }
    }
}
