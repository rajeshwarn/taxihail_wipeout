﻿#region

using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Http;

#endregion

namespace apcurium.MK.Booking.Api.Helpers
{
    public class OrderStatusHelper
    {
        private readonly IOrderDao _orderDao;
        private readonly Resources.Resources _resources;

        public OrderStatusHelper(IOrderDao orderDao, IServerSettings serverSettings)
        {
            _orderDao = orderDao;

            _resources = new Resources.Resources(serverSettings);
        }

        public virtual Task<OrderStatusDetail> GetOrderStatus(Guid orderId, SessionEntity session)
        {
            return Task.Run(() =>
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
                        IBSStatusDescription = _resources.Get("OrderStatus_wosWAITING"),
                    };
                }

                ThrowIfUnauthorized(order, session);

                var o = _orderDao.FindOrderStatusById(orderId);

                return o;
            });
        }

        private static void ThrowIfUnauthorized(OrderDetail order, SessionEntity session)
        {
            if (session.UserId != order.AccountId)
            {
                throw BaseApiService.GenerateException(HttpStatusCode.Unauthorized, "Can't access another account's order");
            }
        }
    }
}