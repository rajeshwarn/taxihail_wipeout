using System;
using System.Net;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Client.Cmt.Payments;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Authorization;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Capture;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments;
using apcurium.MK.Booking.Api.Contract.Requests.Cmt;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Services
{
    public class CmtPaymentService : Service
    {
        readonly ICommandBus _commandBus;
        readonly ICreditCardPaymentDao _dao;
        readonly IOrderDao _orderDao;
        private CmtPaymentServiceClient Client { get; set; }
        public CmtPaymentService(ICommandBus commandBus, ICreditCardPaymentDao dao, IOrderDao orderDao, IConfigurationManager configurationManager)
        {
            _commandBus = commandBus;
            _dao = dao;
            _orderDao = orderDao;

            Client = new CmtPaymentServiceClient(configurationManager.GetPaymentSettings().CmtPaymentSettings, true);
        }


        public DeleteTokenizedCreditcardResponse Delete(DeleteTokenizedCreditcardCmtRequest request)
        {
            var response = Client.Delete(new TokenizeDeleteRequest()
            {
                CardToken = request.CardToken
            });

            return new DeleteTokenizedCreditcardResponse()
            {
                IsSuccessfull = response.ResponseCode == 1,
                Message = response.ResponseMessage
            };
        }

        public PreAuthorizePaymentResponse Post(PreAuthorizePaymentCmtRequest preAuthorizeRequest)
        {
            var orderDetail = _orderDao.FindById(preAuthorizeRequest.OrderId);
            if (orderDetail == null) throw new HttpError(HttpStatusCode.BadRequest, "Order not found");
            if (orderDetail.IBSOrderId == null) throw new HttpError(HttpStatusCode.BadRequest, "Order has no IBSOrderId");

            var request = new AuthorizationRequest()
            {
                Amount = (int)(preAuthorizeRequest.Amount * 100),
                CardOnFileToken = preAuthorizeRequest.CardToken,
                TransactionType = AuthorizationRequest.TransactionTypes.PreAuthorized,
                CardReaderMethod = AuthorizationRequest.CardReaderMethods.Manual,
                L3Data = new LevelThreeData()
                {
                    PurchaseOrderNumber = orderDetail.IBSOrderId.ToString()
                }
            };
            var response = Client.Post(request);

            bool isSuccessful = response.ResponseCode == 1;
            if (isSuccessful)
            {
                _commandBus.Send(new InitiateCreditCardPayment
                {
                    PaymentId = Guid.NewGuid(),
                    TransactionId = response.TransactionId.ToString(),
                    Amount = request.Amount,
                    OrderId = preAuthorizeRequest.OrderId,
                });
            }

            return new PreAuthorizePaymentResponse()
            {
                IsSuccessfull = isSuccessful,
                Message = response.ResponseMessage,
                TransactionId = response.TransactionId + "",
            };

        }


        public CommitPreauthorizedPaymentResponse Post(CommitPreauthorizedPaymentCmtRequest request)
        {
            var payment = _dao.FindByTransactionId(request.TransactionId);
            if(payment == null) throw new HttpError(HttpStatusCode.NotFound, "Payment not found");

            var response = Client.Post(new CaptureRequest
            {
                TransactionId = request.TransactionId.ToLong(),
                L3Data = new LevelThreeData()
                {
                    PurchaseOrderNumber = request.OrderNumber
                }
            });

            bool isSuccessful = response.ResponseCode == 1;
            
            if (isSuccessful)
            {
                _commandBus.Send(new CaptureCreditCardPayment
                {
                    PaymentId = payment.PaymentId,
                });
            }
            return new CommitPreauthorizedPaymentResponse()
            {
                IsSuccessfull = isSuccessful,
                Message = response.ResponseMessage,
            };
        }

    }
}
