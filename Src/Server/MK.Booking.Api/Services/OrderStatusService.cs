#region

using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using AutoMapper;
using ServiceStack.Common.Web;
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

        public object Get(ActiveOrderRequest request)
        {
            var account = _accountDao.FindById(Guid.Parse(this.GetSession().UserAuthId));

            var orderStatusDetails = _orderDao.GetActiveOrderStatusDetails(account.Id);

            if (orderStatusDetails == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "NoActiveOrder");
            }
            
            var orderDetail = _orderDao.FindById(orderStatusDetails.OrderId);

            return new ActivateOrderResponse
            {
                Order = new OrderMapper().ToResource(orderDetail),
                OrderStatusDetail = orderStatusDetails
            };

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