#region

using System.Net;
using System.Threading.Tasks;
using System.Web;
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
    public class OrderPairingService : BaseApiService
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

        public async Task Post(UpdateAutoTipRequest request)
        {
            var orderPairing = _orderDao.FindOrderPairingById(request.OrderId);
            if (orderPairing == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Pairing information not found");
            }

            var order = _orderDao.FindById(request.OrderId);

            var paymentSettings = _serverSettings.GetPaymentSettings(order.CompanyKey);
            if (paymentSettings.PaymentMode == PaymentMethod.RideLinqCmt)
            {
                var result = await _paymentService.UpdateAutoTip(order.CompanyKey, request.OrderId, request.AutoTipPercentage);
                if (!result.IsSuccessful)
                {
                    throw new HttpException(result.Message);
                }
            }

            _commandBus.Send(new UpdateAutoTip
            {
                OrderId = request.OrderId,
                AutoTipPercentage = request.AutoTipPercentage
            });
        }
    }
}