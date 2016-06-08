#region

using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using AutoMapper;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using apcurium.MK.Common.Diagnostic;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderStatusService : Service
    {
        private readonly OrderStatusHelper _orderStatusHelper;
        private readonly ILogger _logger;

        public OrderStatusService(OrderStatusHelper orderStatusHelper,
            ILogger logger)
        {
            _orderStatusHelper = orderStatusHelper;
            _logger = logger;
        }

        public object Get(OrderStatusRequest request)
        {
            AuthService.CurrentSessionFactory();
            var status = _orderStatusHelper.GetOrderStatus(request.OrderId, SessionAs<IAuthSession>());
            _logger.LogMessage("OrderStatusService: Vehicle {0} OrderId: {1}: ({2}, {3}).", status.VehicleNumber, status.OrderId, status.VehicleLatitude, status.VehicleLongitude);

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