#region

using System.Net;
using System.Web;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using AutoMapper;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderPairingService : Service
    {
        private readonly IOrderDao _orderDao;
        private readonly ICommandBus _commandBus;

        public OrderPairingService(IOrderDao orderDao, ICommandBus commandBus)
        {
            _orderDao = orderDao;
            _commandBus = commandBus;
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
                throw new HttpException((int) HttpStatusCode.BadRequest, string.Format("No pairing found for order {0}", request.OrderId));
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