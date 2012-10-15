using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Services
{
    public class SendReceiptService : RestServiceBase<SendReceipt>
    {
        private readonly ICommandBus _commandBus;
        private readonly IBookingWebServiceClient _bookingWebServiceClient;
        private readonly IOrderDao _orderDao;
        private readonly IAccountDao _accountDao;

        public SendReceiptService(ICommandBus commandBus, IBookingWebServiceClient bookingWebServiceClient, IOrderDao orderDao, IAccountDao accountDao)
        {
            _bookingWebServiceClient = bookingWebServiceClient;
            _orderDao = orderDao;
            _accountDao = accountDao;
            _commandBus = commandBus;
        }

        public override object OnPost(SendReceipt request)
        {
            var order = _orderDao.FindById(request.OrderId);
            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            if (!order.IBSOrderId.HasValue)
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.OrderNotInIbs.ToString());
            }

            if (account.Id != order.AccountId)
            {
                throw new HttpError(HttpStatusCode.Unauthorized, "Not your order");
            }

            var orderStatus = _bookingWebServiceClient.GetOrderStatus(order.IBSOrderId.Value, account.IBSAccountId);

            if (orderStatus.Status != "wosDONE")
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.OrderNotCompleted.ToString());
            }

            var IBSOrder = _bookingWebServiceClient.GetOrderDetails(order.IBSOrderId.Value, account.IBSAccountId, order.Settings.Phone);

            var command = new Commands.SendReceipt
            {
                Id = Guid.NewGuid(), 
                OrderId = request.OrderId,
                EmailAddress = account.Email,
                IBSOrderId = order.IBSOrderId.Value,
                VehicleNumber = IBSOrder.VehicleNumber,
                Fare = IBSOrder.Fare.GetValueOrDefault(),
                Toll = IBSOrder.Toll.GetValueOrDefault(),
                Tip = IBSOrder.Tip.GetValueOrDefault(),
            };

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK, "OK");
        }
    }
}
