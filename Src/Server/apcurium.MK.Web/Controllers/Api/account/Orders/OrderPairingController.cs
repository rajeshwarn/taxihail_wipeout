using System;
using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Account.Orders
{
    [RoutePrefix("api/account/orders")]
    [Auth]
    public class OrderPairingController: BaseApiController
    {
        private readonly OrderPairingService _orderPairingService;

        public OrderPairingController(IOrderDao orderDao, ICommandBus commandBus, IServerSettings serverSettings, IPaymentService paymentService)
        {
            _orderPairingService = new OrderPairingService(orderDao, commandBus, serverSettings, paymentService)
            {
                Session = GetSession(),
                HttpRequestContext = RequestContext
            };
        }


        [HttpPost, NoCache, Route("{orderId}/pairing/tip")]
        public async Task<IHttpActionResult> UpdateAutoTip(Guid orderId, [FromBody]UpdateAutoTipRequest request)
        {
            request.OrderId = orderId;

            await _orderPairingService.Post(request);

            return Ok();
        }

        [HttpGet, Route("{OrderId}/pairing"), NoCache]
        public IHttpActionResult GetOrderPairing(Guid orderId)
        {
            var result = _orderPairingService.Get(new OrderPairingRequest() {OrderId = orderId});

            return GenerateActionResult(result);
        }
    }
}