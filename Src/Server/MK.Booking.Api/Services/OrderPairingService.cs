#region

using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using AutoMapper;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Common.Configuration.Impl;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderPairingService : Service
    {
        private readonly IOrderDao _orderDao;
        private readonly ICommandBus _commandBus;
        private readonly IServerSettings _serverSettings;
        private readonly IPaymentService _paymentService;

        public OrderPairingService(IOrderDao orderDao, ICommandBus commandBus, IServerSettings serverSettings, IPaymentService paymentService)
        {
            _orderDao = orderDao;
            _commandBus = commandBus;
            _serverSettings = serverSettings;
            _paymentService = paymentService;
        }

        public object Get(OrderPairingRequest request)
        {
            var orderPairing = _orderDao.FindOrderPairingById(request.OrderId);

            return Mapper.Map<OrderPairingResponse>(orderPairing);
        }

        public object Post(UpdateAutoTipRequest request)
        {
            var orderPairing = _orderDao.FindOrderPairingById(request.OrderId);
            if (orderPairing == null)
            {
                return new HttpResult(HttpStatusCode.NotFound);
            }

            var order = _orderDao.FindById(request.OrderId);

            var paymentSettings = _serverSettings.GetPaymentSettings(order.CompanyKey);
            if (paymentSettings.PaymentMode == PaymentMethod.RideLinqCmt)
            {
                var result = _paymentService.UpdateAutoTip(order.CompanyKey, request.OrderId, request.AutoTipPercentage);
                if (!result.IsSuccessful)
                {
                    return new HttpResult(HttpStatusCode.InternalServerError, result.Message);
                }
            }

            _commandBus.Send(new UpdateAutoTip
            {
                OrderId = request.OrderId,
                AutoTipPercentage = request.AutoTipPercentage
            });

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}