#region

using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using AutoMapper;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderStatusService : Service
    {
        private readonly OrderStatusHelper _orderStatusHelper;

        public OrderStatusService(OrderStatusHelper orderStatusHelper)
        {
            _orderStatusHelper = orderStatusHelper;
        }

        public object Get(OrderStatusRequest request)
        {
            AuthService.CurrentSessionFactory();
            var status = _orderStatusHelper.GetOrderStatus(request.OrderId, SessionAs<IAuthSession>());

            return Mapper.Map<OrderStatusRequestResponse>(status);
        }
    }

    public class ActiveOrderStatusService : Service
    {
        private readonly IAccountDao _accountDao;
        private readonly IOrderDao _orderDao;

        public ActiveOrderStatusService(IOrderDao orderDao, IAccountDao accountDao)
        {
            _accountDao = accountDao;
            _orderDao = orderDao;
        }

        public object Get(ActiveOrderStatusRequest request)
        {
            var statuses = new ActiveOrderStatusRequestResponse();
            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            foreach (var status in _orderDao.GetOrdersInProgressByAccountId(account.Id))
            {
                statuses.Add(status);
            }

            return statuses;
        }
    }
}