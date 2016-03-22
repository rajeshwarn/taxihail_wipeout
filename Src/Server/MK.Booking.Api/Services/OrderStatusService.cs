#region

using System.Threading.Tasks;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using AutoMapper;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderStatusService : BaseApiService
    {
        private readonly OrderStatusHelper _orderStatusHelper;

        public OrderStatusService(OrderStatusHelper orderStatusHelper)
        {
            _orderStatusHelper = orderStatusHelper;
        }

        public async Task<OrderStatusRequestResponse> Get(OrderStatusRequest request)
        {
            var status = await _orderStatusHelper.GetOrderStatus(request.OrderId, Session);

            return Mapper.Map<OrderStatusRequestResponse>(status);
        }
    }

    public class ActiveOrderStatusService : BaseApiService
    {
        private readonly IAccountDao _accountDao;
        private readonly IOrderDao _orderDao;

        public ActiveOrderStatusService(IOrderDao orderDao, IAccountDao accountDao)
        {
            _accountDao = accountDao;
            _orderDao = orderDao;
        }

        public ActivateOrderResponse GetActiveOrder()
        {
            var account = _accountDao.FindById(Session.UserId);

            var orderStatusDetails = _orderDao.GetActiveOrderStatusDetails(account.Id);

            if (orderStatusDetails == null)
            {
                throw GenerateException(HttpStatusCode.NotFound, "NoActiveOrder");
            }
            
            var orderDetail = _orderDao.FindById(orderStatusDetails.OrderId);

            return new ActivateOrderResponse
            {
                Order = new OrderMapper().ToResource(orderDetail),
                OrderStatusDetail = orderStatusDetails
            };

        }

        public ActiveOrderStatusRequestResponse GetActiveOrders()
        {
            var statuses = new ActiveOrderStatusRequestResponse();
            var account = _accountDao.FindById(Session.UserId);

            foreach (var status in _orderDao.GetOrdersInProgressByAccountId(account.Id))
            {
                statuses.Add(status);
            }

            return statuses;
        }
    }
}