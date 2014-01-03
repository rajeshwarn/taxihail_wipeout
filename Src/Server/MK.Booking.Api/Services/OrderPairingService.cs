#region

using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using AutoMapper;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderPairingService : Service
    {
        private readonly IOrderDao _orderDao;

        public OrderPairingService(IOrderDao orderDao)
        {
            _orderDao = orderDao;
        }

        public object Get(OrderPairingRequest request)
        {
            var orderPairing = _orderDao.FindOrderPairingById(request.OrderId);

            return Mapper.Map<OrderPairingResponse>(orderPairing);
        }
    }
}