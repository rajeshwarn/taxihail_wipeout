using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Api.Services
{
    public class SendReceiptService : RestServiceBase<SendReceipt>
    {
        private readonly ICommandBus _commandBus;
        private readonly IBookingWebServiceClient _bookingWebServiceClient;
        private readonly IOrderDao _orderDao;
        private readonly IOrderPaymentDao _orderPaymentDao;
        private readonly IAccountDao _accountDao;        
        private readonly ICreditCardDao _creditCardDao;

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

            var orderStatus = _bookingWebServiceClient.GetOrderStatus(order.IBSOrderId.Value, account.IBSAccountId, order.Settings.Phone);

            if (orderStatus.Status != VehicleStatuses.Common.Done)
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.OrderNotCompleted.ToString());
            }

            var ibsOrder = _bookingWebServiceClient.GetOrderDetails(order.IBSOrderId.Value, account.IBSAccountId, order.Settings.Phone);

            if(ibsOrder.Fare.GetValueOrDefault() < .1)
            {
                throw new HttpError(HttpStatusCode.BadRequest, ErrorCode.OrderNotCompleted.ToString());
            }
           
            var command = new Commands.SendReceipt
            {
                Id = Guid.NewGuid(), 
                OrderId = request.OrderId,
                EmailAddress = account.Email,
                IBSOrderId = order.IBSOrderId.Value,
                TransactionDate = order.PickupDate,
                VehicleNumber = ibsOrder.VehicleNumber,
                Fare = ibsOrder.Fare.GetValueOrDefault(),
                Toll = ibsOrder.Toll.GetValueOrDefault(),
                Tip = ibsOrder.Tip.GetValueOrDefault(),
                Tax = ibsOrder.VAT.GetValueOrDefault(),
                
            };

#warning Copy and paste
            var orderPayement = _orderPaymentDao.FindByOrderId(request.OrderId );
            if (orderPayement != null)
            {
                command.CardOnFileInfo = new Commands.SendReceipt.CardOnFile(
                    orderPayement.Amount,
                    orderPayement.TransactionId,
                    orderPayement.AuthorizationCode,
                    orderPayement.Type == PaymentType.CreditCard ? "Credit Card" : orderPayement.Type.ToString());

                if (orderPayement.CardToken.HasValue())
                {
                    var creditCard = _creditCardDao.FindByToken(orderPayement.CardToken);
                    if (creditCard != null)
                    {
                        command.CardOnFileInfo.LastFour = creditCard.Last4Digits;
                        command.CardOnFileInfo.Company = creditCard.CreditCardCompany;
                        command.CardOnFileInfo.FriendlyName = creditCard.FriendlyName;
                    }
                }

            }


            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK, "OK");
        }
    }
}
