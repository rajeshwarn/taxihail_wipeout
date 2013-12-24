using System;
using System.Net;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface.Auth;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Helpers
{
    public class OrderStatusHelper
    {
        private readonly IOrderDao _orderDao;
        private readonly Guid _accountId;

        public OrderStatusHelper(IOrderDao orderDao)
        {
            _orderDao = orderDao;
        }

        public virtual OrderStatusDetail GetOrderStatus(Guid orderId, IAuthSession session)
        {
            
            var order = _orderDao.FindById(orderId);

            if (order == null)
            {
                //Order could be null if creating the order takes a lot of time and this method is called before the create finishes
                return new OrderStatusDetail
                           {
                               OrderId = orderId,
                               Status = OrderStatus.Created,
                               IBSOrderId = 0,
                               IBSStatusId = "",
                               IBSStatusDescription = "Processing your order"
                           };
            }

            ThrowIfUnauthorized(order, session);


            var o = _orderDao.FindOrderStatusById(orderId);
            
            return o;
        }

        public void ThrowIfUnauthorized(OrderDetail order, IAuthSession session)
        {
            if (new Guid(session.UserAuthId) != order.AccountId)
            {
                throw new HttpError(HttpStatusCode.Unauthorized, "Can't access another account's order");
            }
        }
    }
}