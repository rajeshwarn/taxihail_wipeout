using System;
using System.Globalization;
using System.Net;
using System.Threading;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
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

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class CmtPaymentService : Service
    {
        readonly ICommandBus _commandBus;
        readonly IOrderPaymentDao  _orderPaymentDao;
        readonly IOrderDao _orderDao;
        readonly IConfigurationManager _configurationManager;
        private readonly CmtPaymentServiceClient Client;

        public CmtPaymentService(ICommandBus commandBus, IOrderPaymentDao orderPaymentDao, IOrderDao orderDao, IConfigurationManager configurationManager)
        {
            _commandBus = commandBus;
            _orderPaymentDao = orderPaymentDao;
            _orderDao = orderDao;

            _configurationManager = configurationManager;
            Client = new CmtPaymentServiceClient(configurationManager.GetPaymentSettings().CmtPaymentSettings, null, "Test");
        }

        public DeleteTokenizedCreditcardResponse Delete(DeleteTokenizedCreditcardCmtRequest request)
        {
            var response = Client.Delete(new TokenizeDeleteRequest
            {
                CardToken = request.CardToken
            });

            return new DeleteTokenizedCreditcardResponse
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

                var request =  new AuthorizationRequest
                    {
                        Amount = (int) (preAuthorizeRequest.Amount*100),
                        CardOnFileToken = preAuthorizeRequest.CardToken,
                        TransactionType = AuthorizationRequest.TransactionTypes.PreAuthorized,
                        CardReaderMethod = AuthorizationRequest.CardReaderMethods.Manual,
                        CustomerReferenceNumber =  orderDetail.IBSOrderId.ToString(),
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
                            Amount = Convert.ToDecimal( preAuthorizeRequest.Amount ),
                            OrderId = preAuthorizeRequest.OrderId,
                            Tip = Convert.ToDecimal( preAuthorizeRequest.Tip),
                            Meter = Convert.ToDecimal( preAuthorizeRequest.Meter),
                            CardToken = preAuthorizeRequest.CardToken,
                            Provider = PaymentProvider.CMT,
                        });
                }

                return new PreAuthorizePaymentResponse
                    {
                        IsSuccessfull = isSuccessful,
                        Message = response.ResponseMessage,
                        TransactionId = response.TransactionId.ToString(),
                    };
            }
            catch (Exception e)
            {
                return new PreAuthorizePaymentResponse
                    {
                        IsSuccessfull = false,
                        Message = e.Message,
                    };
            }
        }

        public CommitPreauthorizedPaymentResponse Post(PreAuthorizeAndCommitPaymentCmtRequest request)
        {
            try
            {
                var isSuccessful = false;
                var authorizationCode = string.Empty;
                var message = string.Empty;

                var orderDetail = _orderDao.FindById(request.OrderId);
                if (orderDetail == null) throw new HttpError(HttpStatusCode.BadRequest, "Order not found");
                if (orderDetail.IBSOrderId == null)
                    throw new HttpError(HttpStatusCode.BadRequest, "Order has no IBSOrderId");

                var authRequest = new AuthorizationRequest
                {
                    Amount = (int)(request.Amount * 100),
                    CardOnFileToken = request.CardToken,
                    TransactionType = AuthorizationRequest.TransactionTypes.PreAuthorized,
                    CardReaderMethod = AuthorizationRequest.CardReaderMethods.Manual,
                    CustomerReferenceNumber = orderDetail.IBSOrderId.ToString(),
                    MerchantToken = _configurationManager.GetPaymentSettings().CmtPaymentSettings.MerchantToken
                };

                var authResponse = Client.Post(authRequest);
                message = authResponse.ResponseMessage;

                if (authResponse.ResponseCode == 1)
                {
                    var transactionId = authResponse.TransactionId.ToString(CultureInfo.InvariantCulture);
                    var paymentId = Guid.NewGuid();

                    _commandBus.Send(new InitiateCreditCardPayment
                    {
                        PaymentId = paymentId,
                        TransactionId = transactionId,
                        Amount = Convert.ToDecimal(request.Amount),
                        OrderId = request.OrderId,
                        Tip = Convert.ToDecimal(request.TipAmount),
                        Meter = Convert.ToDecimal(request.MeterAmount),
                        CardToken = request.CardToken,
                        Provider = PaymentProvider.CMT,
                    });

                    // wait for OrderPaymentDetail to be created
                    Thread.Sleep(500);

                    // commit transaction
                    var captureResponse = Client.Post(new CaptureRequest
                    {
                        MerchantToken = _configurationManager.GetPaymentSettings().CmtPaymentSettings.MerchantToken,
                        TransactionId = transactionId.ToLong(),
                    });

                    message = captureResponse.ResponseMessage;
                    isSuccessful = captureResponse.ResponseCode == 1;
                    if (isSuccessful)
                    {
                        authorizationCode = captureResponse.AuthorizationCode;

                        _commandBus.Send(new CaptureCreditCardPayment
                        {
                            PaymentId = paymentId,
                            AuthorizationCode = authorizationCode,
                            Provider = PaymentProvider.CMT,
                        });
                    }
                }

                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessfull = isSuccessful,
                    Message = message,
                    AuthorizationCode = authorizationCode,
                };
            }
            catch (Exception e)
            {
                return new CommitPreauthorizedPaymentResponse
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
                var payment = _orderPaymentDao.FindByTransactionId(request.TransactionId);
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
                            AuthorizationCode = response.AuthorizationCode,
                            Provider = PaymentProvider.CMT,
                        });
                }

                return new CommitPreauthorizedPaymentResponse
                    {
                        IsSuccessfull = isSuccessful,
                        Message = response.ResponseMessage,
                        AuthorizationCode = response.AuthorizationCode,
                    };
            }
            catch (Exception e)
            {
                return new CommitPreauthorizedPaymentResponse
                    {
                        IsSuccessfull = false,
                        Message = e.Message,
                    };
            }
        }
    }
}
