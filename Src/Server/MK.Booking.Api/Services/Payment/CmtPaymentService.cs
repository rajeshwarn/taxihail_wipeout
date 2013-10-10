using System;
using System.Globalization;
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
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Services
{
    public class CmtPaymentService : Service
    {
        readonly ICommandBus _commandBus;
        readonly ICreditCardPaymentDao _dao;
        readonly IOrderDao _orderDao;
        private IConfigurationManager _configurationManager;
        private CmtPaymentServiceClient Client;

        public CmtPaymentService(ICommandBus commandBus, ICreditCardPaymentDao dao, IOrderDao orderDao, IConfigurationManager configurationManager)
        {
            _commandBus = commandBus;
            _dao = dao;
            _orderDao = orderDao;

            _configurationManager = configurationManager;
            Client = new CmtPaymentServiceClient(configurationManager.GetPaymentSettings().CmtPaymentSettings, null);
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
            try
            {

                var orderDetail = _orderDao.FindById(preAuthorizeRequest.OrderId);
                if (orderDetail == null) throw new HttpError(HttpStatusCode.BadRequest, "Order not found");
                if (orderDetail.IBSOrderId == null)
                    throw new HttpError(HttpStatusCode.BadRequest, "Order has no IBSOrderId");

                var request =  new AuthorizationRequest()
                    {
                        Amount = (int) (preAuthorizeRequest.Amount*100),
                        CardOnFileToken = preAuthorizeRequest.CardToken,
                        TransactionType = AuthorizationRequest.TransactionTypes.PreAuthorized,
                        CardReaderMethod = AuthorizationRequest.CardReaderMethods.Manual,
                        L3Data = new LevelThreeData()
                            {
                                PurchaseOrderNumber = orderDetail.IBSOrderId.ToString()
                            },
                        MerchantToken = _configurationManager.GetPaymentSettings().CmtPaymentSettings.MerchantToken

                    };
                var response = Client.Post(request);

                var isSuccessful = response.ResponseCode == 1;
                if (isSuccessful)
                {
                    _commandBus.Send(new InitiateCreditCardPayment
                        {
                            PaymentId = Guid.NewGuid(),
                            TransactionId = response.TransactionId.ToString(CultureInfo.InvariantCulture),
                            Amount = request.Amount,
                            OrderId = preAuthorizeRequest.OrderId,
                            CardToken = preAuthorizeRequest.CardToken
                        });
                }

                return new PreAuthorizePaymentResponse()
                    {
                        IsSuccessfull = isSuccessful,
                        Message = response.ResponseMessage,
                        TransactionId = response.TransactionId + "",
                    };
            }
            catch (Exception e)
            {
                return new PreAuthorizePaymentResponse()
                    {
                        IsSuccessfull = false,
                        Message = e.Message,
                    };
            }
        }


        public CommitPreauthorizedPaymentResponse Post(CommitPreauthorizedPaymentCmtRequest request)
        {
            try
            {
                var payment = _dao.FindByTransactionId(request.TransactionId);
                if (payment == null) throw new HttpError(HttpStatusCode.NotFound, "Payment not found");
                
                var response = Client.Post(new CaptureRequest
                    {
                        MerchantToken = _configurationManager.GetPaymentSettings().CmtPaymentSettings.MerchantToken,
                        TransactionId = request.TransactionId.ToLong(),
                    });

                var isSuccessful = response.ResponseCode == 1;

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
            catch (Exception e)
            {
                return new CommitPreauthorizedPaymentResponse()
                    {
                        IsSuccessfull = false,
                        Message = e.Message,
                    };
            }
        }

    
    }
}
