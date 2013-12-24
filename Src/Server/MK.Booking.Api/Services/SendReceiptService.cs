#region

using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.CommandBuilder;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class SendReceiptService : Service
    {
        private readonly IAccountDao _accountDao;
        private readonly IBookingWebServiceClient _bookingWebServiceClient;
        private readonly ICommandBus _commandBus;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IOrderDao _orderDao;
        private readonly IOrderPaymentDao _orderPaymentDao;

        public SendReceiptService(
            ICommandBus commandBus,
            IBookingWebServiceClient bookingWebServiceClient,
            IOrderDao orderDao,
            IOrderPaymentDao orderPaymentDao,
            ICreditCardDao creditCardDao,
            IAccountDao accountDao
            )
        {
            _bookingWebServiceClient = bookingWebServiceClient;
            _orderDao = orderDao;
            _orderPaymentDao = orderPaymentDao;
            _accountDao = accountDao;
            _creditCardDao = creditCardDao;
            _commandBus = commandBus;
        }

        public object Post(SendReceipt request)
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

            var ibsOrder = _bookingWebServiceClient.GetOrderDetails(order.IBSOrderId.Value, account.IBSAccountId,
                order.Settings.Phone);

            var orderPayment = _orderPaymentDao.FindByOrderId(order.Id);

            if ((orderPayment != null) && (orderPayment.IsCompleted))
            {
                var creditCard = orderPayment.CardToken.HasValue()
                    ? _creditCardDao.FindByToken(orderPayment.CardToken)
                    : null;
                _commandBus.Send(SendReceiptCommandBuilder.GetSendReceiptCommand(order, account, ibsOrder.VehicleNumber,
                    Convert.ToDouble(orderPayment.Meter), 0, Convert.ToDouble(orderPayment.Tip), 0, orderPayment,
                    creditCard));
            }
            else
            {
                _commandBus.Send(SendReceiptCommandBuilder.GetSendReceiptCommand(order, account, ibsOrder.VehicleNumber,
                    ibsOrder.Fare, ibsOrder.Toll, ibsOrder.Tip, ibsOrder.VAT));
            }

            return new HttpResult(HttpStatusCode.OK, "OK");
        }
        
    }
}