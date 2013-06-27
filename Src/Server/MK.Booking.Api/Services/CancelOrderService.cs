﻿using System;
using System.Net;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Services
{
    public class CancelOrderService : RestServiceBase<CancelOrder>
    {
        private readonly ICommandBus _commandBus;
        private readonly IBookingWebServiceClient _bookingWebServiceClient;
        private readonly IOrderDao _orderDao;
        private readonly IAccountDao _accountDao;

        public CancelOrderService(ICommandBus commandBus, IBookingWebServiceClient bookingWebServiceClient, IOrderDao orderDao, IAccountDao accountDao)
        {
            _bookingWebServiceClient = bookingWebServiceClient;
            _orderDao = orderDao;
            _accountDao = accountDao;
            _commandBus = commandBus;
        }

        public override object OnPost(CancelOrder request)
        {
            var order = _orderDao.FindById(request.OrderId);
            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            if (order == null)
            {
                return new HttpResult(HttpStatusCode.NotFound);
            }
            
            if (!order.IBSOrderId.HasValue)
            {
                throw new HttpError(ErrorCode.OrderNotInIbs.ToString());
            }

            if (account.Id != order.AccountId)
            {
                throw new HttpError(HttpStatusCode.Unauthorized, "Can't cancel another account's order");
            }

            var isSuccessful = _bookingWebServiceClient.CancelOrder(order.IBSOrderId.Value, account.IBSAccountId, order.Settings.Phone);

            if (!isSuccessful)
            {
                isSuccessful = _bookingWebServiceClient.CancelOrder(order.IBSOrderId.Value, account.IBSAccountId, order.Settings.Phone);
                if (!isSuccessful)
                {
                    throw new HttpError(ErrorCode.OrderNotInIbs.ToString());
                }
            }

            var command = new Commands.CancelOrder {Id = Guid.NewGuid(), OrderId = request.OrderId};
            _commandBus.Send(command);
            
            return new HttpResult(HttpStatusCode.OK);
        }
    }
}
