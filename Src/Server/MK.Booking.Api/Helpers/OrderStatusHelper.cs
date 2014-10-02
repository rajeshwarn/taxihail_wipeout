#region

using System;
using System.Net;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Entity;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface.Auth;
using apcurium.MK.Common.Configuration;

#endregion

namespace apcurium.MK.Booking.Api.Helpers
{
    public class OrderStatusHelper
    {
        private readonly IOrderDao _orderDao;
        private readonly Resources.Resources _resources;

        public OrderStatusHelper(IOrderDao orderDao, IConfigurationManager configurationManager, IAppSettings appSettings)
        {
            _orderDao = orderDao;

            _resources = new Resources.Resources(configurationManager.ServerData.TaxiHail.ApplicationKey, appSettings);
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
                    IBSOrderId =  0,
                    IBSStatusId = "",
                    IBSStatusDescription = _resources.Get("OrderStatus_wosWAITING"),
                };
            }

            ThrowIfUnauthorized(order, session);

            var o = _orderDao.FindOrderStatusById(orderId);

            return o;
        }

        private static void ThrowIfUnauthorized(OrderDetail order, IAuthSession session)
        {
            if (new Guid(session.UserAuthId) != order.AccountId)
            {
                throw new HttpError(HttpStatusCode.Unauthorized, "Can't access another account's order");
            }
        }
    }
}