using AutoMapper;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderPairingService : RestServiceBase<OrderPairingRequest>
    {
        private readonly IOrderDao _orderDao;

        public OrderPairingService(IOrderDao orderDao)
        {
            _orderDao = orderDao;
        }

        public override object OnGet(OrderPairingRequest request)
        {
            var orderPairing = _orderDao.FindOrderPairingById(request.OrderId);

            return Mapper.Map<OrderPairingResponse>(orderPairing);
        }
    }
}