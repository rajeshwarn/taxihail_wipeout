﻿using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
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
    [Auth]
    public class OrderPairingController: BaseApiController
    {
        private readonly OrderPairingService _orderPairingService;

        public OrderPairingController(IOrderDao orderDao, ICommandBus commandBus, IServerSettings serverSettings, IPaymentService paymentService)
        {
            _orderPairingService = new OrderPairingService(orderDao, commandBus, serverSettings, paymentService);

            PrepareApiServices(_orderPairingService);
        }
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            PrepareApiServices(_orderPairingService);
        }


        [HttpPost, NoCache, Route("api/v2/accounts/orders/{orderId}/pairing/tip")]
        public async Task<IHttpActionResult> UpdateAutoTip(Guid orderId, [FromBody]UpdateAutoTipRequest request)
        {
            request.OrderId = orderId;

            await _orderPairingService.Post(request);

            return Ok();
        }

        [HttpGet, Route("api/v2/accounts/orders/{orderId}/pairing"), NoCache]
        public IHttpActionResult GetOrderPairing(Guid orderId)
        {
            var result = _orderPairingService.Get(new OrderPairingRequest() {OrderId = orderId});

            return GenerateActionResult(result);
        }
    }
}