using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Pair;
using apcurium.MK.Booking.Api.Contract.Resources.Payments.Cmt;
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
        private readonly CmtPaymentServiceClient CmtPaymentServiceClient;
        private readonly CmtMobileServiceClient CmtMobileServiceClient;

        public CmtPaymentService(ICommandBus commandBus, IOrderPaymentDao orderPaymentDao, IOrderDao orderDao, IConfigurationManager configurationManager)
        {
            _commandBus = commandBus;
            _orderPaymentDao = orderPaymentDao;
            _orderDao = orderDao;

            _configurationManager = configurationManager;
            CmtPaymentServiceClient = new CmtPaymentServiceClient(configurationManager.GetPaymentSettings().CmtPaymentSettings, null, "TaxiHail");
            CmtMobileServiceClient = new CmtMobileServiceClient(configurationManager.GetPaymentSettings().CmtPaymentSettings, null, "TaxiHail");
        }

        public DeleteTokenizedCreditcardResponse Delete(DeleteTokenizedCreditcardCmtRequest request)
        {
            var response = CmtPaymentServiceClient.Delete(new TokenizeDeleteRequest
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

                var response = CmtPaymentServiceClient.Post(request);

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

                var authResponse = CmtPaymentServiceClient.Post(authRequest);
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
                    var captureResponse = CmtPaymentServiceClient.Post(new CaptureRequest
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

                var response = CmtPaymentServiceClient.Post(new CaptureRequest
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

        public PairingResponse Post(PairingRidelinqCmtRequest request)
        {
            try
            {
                // send pairing request
                var response = CmtMobileServiceClient.Post(new PairingRequest
                {
                    AutoTipAmount = request.AutoTipAmount,
                    AutoTipPercentage = request.AutoTipPercentage,
                    AutoCompletePayment = request.AutoCompletePayment,
                    CallbackUrl = "", // todo wait for confirmation of what we will receive at this callback
                    CustomerId = request.CustomerId,
                    CustomerName = request.CustomerName,
                    DriverId = request.DriverId,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Medallion = request.Medallion
                });

                // fetch trip until 
                try
                {
                    var watch = new Stopwatch();
                    watch.Start();
                    var trip = GetTrip(response.PairingToken);
                    while (trip == null)
                    {
                        Thread.Sleep(2000);
                        trip = GetTrip(response.PairingToken);

                        if (watch.Elapsed.TotalSeconds >= response.TimeoutSeconds)
                        {
                            throw new TimeoutException("Could not be paired with vehicle");
                        }
                    }

                    // todo send a command to save the pairing state for this order
                    return new PairingResponse
                    {
                        IsSuccessfull = true,
                        Message = "Success",
                        PairingToken = response.PairingToken,
                        PairingCode = response.PairingCode,
                        Medallion = response.Medallion,
                        TripId = response.TripId,
                        DriverId = response.DriverId
                    };
                }
                catch (Exception e)
                {
                    return new PairingResponse
                    {
                        IsSuccessfull = false,
                        Message = e.Message
                    };
                }
            }
            catch (WebException e)
            {
                var x = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();

                return new PairingResponse
                {
                    IsSuccessfull = false,
                    Message = e.Message
                };
            }
        }

        private Trip GetTrip(string pairingToken)
        {
            try
            {
                var trip = CmtMobileServiceClient.Get(new TripRequest{ Token = pairingToken });
                if (trip != null)
                {
                    //ugly fix for deserilization problem in datetime on the device for IOS
                    if (trip.StartTime.HasValue)
                    {
                        trip.StartTime = DateTime.SpecifyKind(trip.StartTime.Value, DateTimeKind.Local);
                    }

                    if (trip.EndTime.HasValue)
                    {
                        trip.EndTime = DateTime.SpecifyKind(trip.EndTime.Value, DateTimeKind.Local);
                    }
                }

                return trip;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
