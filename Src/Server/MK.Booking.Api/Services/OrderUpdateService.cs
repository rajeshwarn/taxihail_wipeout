using System.Net;
using System.Web;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderUpdateService : BaseApiService
    {
        private readonly ICommandBus _commandBus;
        private readonly IOrderDao _orderDao;
        private readonly IIBSServiceProvider _ibsServiceProvider;

        public OrderUpdateService(IOrderDao orderDao, ICommandBus commandBus, IIBSServiceProvider ibsServiceProvider)
        {
            _orderDao = orderDao;
            _commandBus = commandBus;
            _ibsServiceProvider = ibsServiceProvider;
        }

        public bool Post(OrderUpdateRequest request)
        {
            var order = _orderDao.FindById(request.OrderId);
            if (order == null || !order.IBSOrderId.HasValue)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, ErrorCode.OrderNotInIbs.ToString());
            }

            var success = _ibsServiceProvider.Booking(order.CompanyKey).UpdateDropOffInTrip(order.IBSOrderId.Value, order.Id, request.DropOffAddress);
            
            if (success)
            {
                _commandBus.Send(new UpdateOrderInTrip { OrderId = request.OrderId, DropOffAddress = request.DropOffAddress });
            }

            return success;
        }
    }
}
