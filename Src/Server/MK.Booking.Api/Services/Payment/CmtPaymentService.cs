using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Pair;
using apcurium.MK.Booking.Api.Contract.Resources.Payments.Cmt;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Common.Diagnostic;
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
using ServiceStack.Text;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class CmtPaymentService : Service
    {
        readonly ICommandBus _commandBus;
        readonly IOrderPaymentDao  _orderPaymentDao;
        readonly IOrderDao _orderDao;
        private readonly IAccountDao _accountDao;
        readonly IConfigurationManager _configurationManager;
        private readonly ILogger _logger;
        private readonly CmtPaymentServiceClient CmtPaymentServiceClient;
        private readonly CmtMobileServiceClient CmtMobileServiceClient;

        public CmtPaymentService(ICommandBus commandBus, IOrderPaymentDao orderPaymentDao, IOrderDao orderDao, IAccountDao accountDao, IConfigurationManager configurationManager, ILogger logger)
        {
            _commandBus = commandBus;
            _orderPaymentDao = orderPaymentDao;
            _orderDao = orderDao;
            _accountDao = accountDao;

            _configurationManager = configurationManager;
            _logger = logger;
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

                var orderStatus = _orderDao.FindOrderStatusById(orderDetail.Id);
                if (orderStatus == null) throw new HttpError(HttpStatusCode.BadRequest, "Order status not found");

                var request =  new AuthorizationRequest
                    {
                        Amount = (int) (preAuthorizeRequest.Amount*100),
                        CardOnFileToken = preAuthorizeRequest.CardToken,
                        TransactionType = AuthorizationRequest.TransactionTypes.PreAuthorized,
                        CardReaderMethod = AuthorizationRequest.CardReaderMethods.Manual,                        
                        MerchantToken = _configurationManager.GetPaymentSettings().CmtPaymentSettings.MerchantToken,
                        CustomerReferenceNumber = string.IsNullOrEmpty(orderStatus.ReferenceNumber) ? orderDetail.IBSOrderId.ToString() : orderStatus.ReferenceNumber,
                        EmployeeId = orderStatus.DriverInfos == null ? "" : orderStatus.DriverInfos.DriverId,
                        DeviceName = orderStatus.TerminalId
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


                var orderStatus = _orderDao.FindOrderStatusById(orderDetail.Id);
                if  ( orderStatus == null )  throw new HttpError(HttpStatusCode.BadRequest, "Order status not found");

                var authRequest = new AuthorizationRequest
                {
                    Amount = (int)(request.Amount * 100),
                    CardOnFileToken = request.CardToken,
                    TransactionType = AuthorizationRequest.TransactionTypes.PreAuthorized,
                    CardReaderMethod = AuthorizationRequest.CardReaderMethods.Manual,                    
                    MerchantToken = _configurationManager.GetPaymentSettings().CmtPaymentSettings.MerchantToken,
                    CustomerReferenceNumber = string.IsNullOrEmpty(orderStatus.ReferenceNumber) ? orderDetail.IBSOrderId.ToString() : orderStatus.ReferenceNumber,
                    EmployeeId = orderStatus.DriverInfos == null ? "" : orderStatus.DriverInfos.DriverId,                    
                    DeviceName = orderStatus.TerminalId,
                    
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
                var orderStatusDetail = _orderDao.FindOrderStatusById(request.OrderId);
                if (orderStatusDetail == null) throw new HttpError(HttpStatusCode.BadRequest, "Order not found");
                if (orderStatusDetail.IBSOrderId == null)
                    throw new HttpError(HttpStatusCode.BadRequest, "Order has no IBSOrderId");

                var accountDetail = _accountDao.FindById(orderStatusDetail.AccountId);

                // send pairing request

                // Determine the root path to the app 
                //var root = ApplicationPathResolver.GetApplicationPath(RequestContext);
                
                var pairingRequest = new PairingRequest
                {
                    AutoTipAmount = request.AutoTipAmount,
                    AutoTipPercentage = request.AutoTipPercentage,
                    AutoCompletePayment = true,
                    CallbackUrl = "",
                    CustomerId = orderStatusDetail.IBSOrderId.ToString(),
                    CustomerName = accountDetail.Name,
                    DriverId = orderStatusDetail.DriverInfos.DriverId,
                    Latitude = orderStatusDetail.VehicleLatitude.GetValueOrDefault(),
                    Longitude = orderStatusDetail.VehicleLongitude.GetValueOrDefault(),
                    Medallion = orderStatusDetail.VehicleNumber,
                    CardOnFileId = request.CardToken
                };


                _logger.LogMessage( "Pairing request : " + pairingRequest.ToJson());
                var response = CmtMobileServiceClient.Post(pairingRequest);
                _logger.LogMessage("Pairing response : " + response.ToJson());
                
                    
                // wait for trip to be updated
                var watch = new Stopwatch();
                watch.Start();
                var trip = GetTrip(response.PairingToken);
                while (trip == null)
                {
                    Thread.Sleep(2000);
                    trip = GetTrip(response.PairingToken);

                    if (watch.Elapsed.TotalSeconds >= response.TimeoutSeconds)
                    {
                        _logger.LogMessage("Timeout Exception, Could not be paired with vehicle.");
                        throw new TimeoutException("Could not be paired with vehicle");
                    }
                }

                // send a command to save the pairing state for this order
                _commandBus.Send(new PairForRideLinqCmtPayment
                    {
                        OrderId = request.OrderId,
                        Medallion = response.Medallion,
                        DriverId = response.DriverId.ToString(),
                        PairingToken = response.PairingToken,
                        PairingCode = response.PairingCode,
                        TokenOfCardToBeUsedForPayment = request.CardToken,
                        AutoTipAmount = request.AutoTipAmount,
                        AutoTipPercentage = request.AutoTipPercentage
                    });

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

        public BasePaymentResponse Post(UnpairingRidelinqCmtRequest request)
        {
            try
            {
                var orderPairingDetail = _orderDao.FindOrderPairingById(request.OrderId);
                if (orderPairingDetail == null) throw new HttpError(HttpStatusCode.BadRequest, "Order not found");

                // send unpairing request
                var response = CmtMobileServiceClient.Delete(new UnpairingRequest
                {
                    PairingToken = orderPairingDetail.PairingToken
                });

                // wait for trip to be updated
                var watch = new Stopwatch();
                watch.Start();
                var trip = GetTrip(orderPairingDetail.PairingToken);
                while (trip != null)
                {
                    Thread.Sleep(2000);
                    trip = GetTrip(orderPairingDetail.PairingToken);

                    if (watch.Elapsed.TotalSeconds >= response.TimeoutSeconds)
                    {
                        throw new TimeoutException("Could not be unpaired of vehicle");
                    }
                }

                // send a command to delete the pairing pairing info for this order
                _commandBus.Send(new UnpairForRideLinqCmtPayment
                {
                    OrderId = request.OrderId
                });

                return new BasePaymentResponse
                {
                    IsSuccessfull = true,
                    Message = "Success"
                };
            }
            catch (Exception e)
            {
                return new BasePaymentResponse
                {
                    IsSuccessfull = false,
                    Message = e.Message
                };
            }
        }

        public void Post(CallbackRequest request)
        {
            try
            {
                if(request == null) throw new Exception("Received callback but couldn't cast to CallbackRequest/Trip");

                if (request.Type == "TRIP")
                {
                    // this is the end of trip event
                    var orderPairingDetail = _orderDao.FindOrderPairingById(request.OrderId);
                    
                    Post(new PreAuthorizeAndCommitPaymentCmtRequest
                        {
                            OrderId = request.OrderId,
                            Amount = request.Fare + request.Tip,
                            MeterAmount = request.Fare,
                            TipAmount = request.Tip,
                            CardToken = orderPairingDetail.TokenOfCardToBeUsedForPayment
                        });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e);
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
