using apcurium.MK.Booking.ReadModel.Query.Contract;
using AutoMapper;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.ReadModel.Query;
using System;

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderStatusService : RestServiceBase<OrderStatusRequest>
    {
        private readonly IAccountDao _accountDao;
        private readonly OrderStatusHelper _orderStatusHelper;

        public OrderStatusService(IAccountDao accountDao, OrderStatusHelper orderStatusHelper)
        {
            _accountDao = accountDao;
            _orderStatusHelper = orderStatusHelper;
        }

        public override object OnGet(OrderStatusRequest request)
        {
            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));
            AuthService.CurrentSessionFactory();
            var status = _orderStatusHelper.GetOrderStatus(request.OrderId, SessionAs<IAuthSession>());

            return Mapper.Map<OrderStatusRequestResponse>(status);
        }
    }

    public class ActiveOrderStatusService : RestServiceBase<Contract.Requests.ActiveOrderStatusRequest>
    {
        private readonly IOrderDao _orderDao;
        private readonly IAccountDao _accountDao;

        public ActiveOrderStatusService(IOrderDao orderDao, IAccountDao accountDao)
        {
            _accountDao = accountDao;
            _orderDao = orderDao;
        }

        public override object OnGet(Contract.Requests.ActiveOrderStatusRequest request)
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
