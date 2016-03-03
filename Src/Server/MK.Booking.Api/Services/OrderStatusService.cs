#region

using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
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

        public ActiveOrderStatusRequestResponse Get()
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