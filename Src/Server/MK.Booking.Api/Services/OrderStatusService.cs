using System.Net;
using AutoMapper;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query;
using System;
using OrderStatusDetail = apcurium.MK.Common.Entity.OrderStatusDetail;

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderStatusService : RestServiceBase<Contract.Requests.OrderStatusRequest>
    {
        private readonly IOrderDao _orderDao;
        private readonly IAccountDao _accountDao;

        public OrderStatusService(IOrderDao orderDao, IAccountDao accountDao)
        {
            _accountDao = accountDao;
            _orderDao = orderDao;
        }

        public override object OnGet(OrderStatusRequest request)
        {
            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            var status = new OrderStatusHelper(_orderDao, account.Id)
                .GetOrderStatus(request.OrderId);

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

    class OrderStatusHelper
    {
        private readonly IOrderDao _orderDao;
        private readonly Guid _accountId;

        public OrderStatusHelper(IOrderDao orderDao, Guid accountId)
        {
            _orderDao = orderDao;
            _accountId = accountId;
        }

        internal OrderStatusDetail GetOrderStatus(Guid id)
        {
            var order = _orderDao.FindById(id);

            if (order == null)
            {
                //Order could be null if creating the order takes a lot of time and this method is called before the create finishes
                return new OrderStatusDetail
                {
                    OrderId = id,
                    Status = apcurium.MK.Common.Entity.OrderStatus.Created,
                    IBSOrderId = 0,
                    IBSStatusId = "",
                    IBSStatusDescription = "Processing your order"
                };
            }

            if (_accountId != order.AccountId)
            {
                throw new HttpError(HttpStatusCode.Unauthorized, "Can't access another account's order");
            }

            if (!order.IBSOrderId.HasValue)
            {
                return new OrderStatusDetail {IBSStatusDescription = "Can't contact dispatch company"};
            }

            return _orderDao.FindOrderStatusById(id);
        }
    }
}
