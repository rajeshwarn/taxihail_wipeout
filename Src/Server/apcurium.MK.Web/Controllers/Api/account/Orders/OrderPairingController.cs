using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Web.Security;
using AutoMapper;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Account.Orders
{
    [RoutePrefix("api/account/orders")]
    [Auth]
    public class OrderPairingController: BaseApiController
    {
        private readonly IOrderDao _orderDao;
        private readonly ICommandBus _commandBus;
        private readonly IServerSettings _serverSettings;
        private readonly IPaymentService _paymentService;

        public OrderPairingController(IOrderDao orderDao, ICommandBus commandBus, IServerSettings serverSettings, IPaymentService paymentService)
        {
            _orderDao = orderDao;
            _commandBus = commandBus;
            _serverSettings = serverSettings;
            _paymentService = paymentService;
        }


        [HttpPost, NoCache, Route("{OrderId}/pairing/tip")]
        public async Task<object> UpdateAutoTip(UpdateAutoTipRequest request)
        {
            var orderPairing = _orderDao.FindOrderPairingById(request.OrderId);
            if (orderPairing == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var order = _orderDao.FindById(request.OrderId);

            var paymentSettings = _serverSettings.GetPaymentSettings(order.CompanyKey);
            if (paymentSettings.PaymentMode == PaymentMethod.RideLinqCmt)
            {
                var result = await _paymentService.UpdateAutoTip(order.CompanyKey, request.OrderId, request.AutoTipPercentage);
                if (!result.IsSuccessful)
                {
                    throw new HttpException((int)HttpStatusCode.InternalServerError, result.Message);
                }
            }

            _commandBus.Send(new UpdateAutoTip
            {
                OrderId = request.OrderId,
                AutoTipPercentage = request.AutoTipPercentage
            });

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet, Route("{OrderId}/pairing"), NoCache]
        public OrderPairingResponse GetOrderPairing(Guid orderId)
        {
            var orderPairing = _orderDao.FindOrderPairingById(orderId);

            return Mapper.Map<OrderPairingResponse>(orderPairing);
        }
    }
}