﻿#region

using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Threading;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments.Authorization;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments.Capture;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments.Pair;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments.Tokenize;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.Cmt;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Api.Contract.Resources.Payments.Cmt;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class CmtPaymentService : Service
    {
        private readonly CmtMobileServiceClient _cmtMobileServiceClient;
        private readonly CmtPaymentServiceClient _cmtPaymentServiceClient;
        private readonly IAccountDao _accountDao;
        private readonly ICommandBus _commandBus;
        private readonly IConfigurationManager _configurationManager;
        private readonly ILogger _logger;
        private readonly IOrderDao _orderDao;
        private readonly IOrderPaymentDao _orderPaymentDao;

        public CmtPaymentService(ICommandBus commandBus, IOrderPaymentDao orderPaymentDao, IOrderDao orderDao,
            IAccountDao accountDao, IConfigurationManager configurationManager, ILogger logger)
        {
            _commandBus = commandBus;
            _orderPaymentDao = orderPaymentDao;
            _orderDao = orderDao;
            _accountDao = accountDao;

            _configurationManager = configurationManager;
            _logger = logger;
            _cmtPaymentServiceClient =
                new CmtPaymentServiceClient(configurationManager.GetPaymentSettings().CmtPaymentSettings, null,
                    "TaxiHail");
            _cmtMobileServiceClient =
                new CmtMobileServiceClient(configurationManager.GetPaymentSettings().CmtPaymentSettings, null,
                    "TaxiHail");
        }

        public DeleteTokenizedCreditcardResponse Delete(DeleteTokenizedCreditcardCmtRequest request)
        {
            var response = _cmtPaymentServiceClient.Delete(new TokenizeDeleteRequest
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

                var request = new AuthorizationRequest
                {
                    Amount = (int) (preAuthorizeRequest.Amount*100),
                    CardOnFileToken = preAuthorizeRequest.CardToken,
                    TransactionType = AuthorizationRequest.TransactionTypes.PreAuthorized,
                    CardReaderMethod = AuthorizationRequest.CardReaderMethods.Manual,
                    CustomerReferenceNumber = orderDetail.IBSOrderId.ToString(),
                    MerchantToken = _configurationManager.GetPaymentSettings().CmtPaymentSettings.MerchantToken
                };

                var response = _cmtPaymentServiceClient.Post(request);

                var isSuccessful = response.ResponseCode == 1;
                if (isSuccessful)
                {
                    _commandBus.Send(new InitiateCreditCardPayment
                    {
                        PaymentId = Guid.NewGuid(),
                        TransactionId = response.TransactionId.ToString(CultureInfo.InvariantCulture),
                        Amount = Convert.ToDecimal(preAuthorizeRequest.Amount),
                        OrderId = preAuthorizeRequest.OrderId,
                        Tip = Convert.ToDecimal(preAuthorizeRequest.Tip),
                        Meter = Convert.ToDecimal(preAuthorizeRequest.Meter),
                        CardToken = preAuthorizeRequest.CardToken,
                        Provider = PaymentProvider.CMT,
                    });
                }

                return new PreAuthorizePaymentResponse
                {
                    IsSuccessfull = isSuccessful,
                    Message = response.ResponseMessage,
                    TransactionId = response.TransactionId.ToString(CultureInfo.InvariantCulture),
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
                string message;

                var orderDetail = _orderDao.FindById(request.OrderId);
                if (orderDetail == null) throw new HttpError(HttpStatusCode.BadRequest, "Order not found");
                if (orderDetail.IBSOrderId == null)
                    throw new HttpError(HttpStatusCode.BadRequest, "Order has no IBSOrderId");

                var authRequest = new AuthorizationRequest
                {
                    Amount = (int) (request.Amount*100),
                    CardOnFileToken = request.CardToken,
                    TransactionType = AuthorizationRequest.TransactionTypes.PreAuthorized,
                    CardReaderMethod = AuthorizationRequest.CardReaderMethods.Manual,
                    CustomerReferenceNumber = orderDetail.IBSOrderId.ToString(),
                    MerchantToken = _configurationManager.GetPaymentSettings().CmtPaymentSettings.MerchantToken
                };

                var authResponse = _cmtPaymentServiceClient.Post(authRequest);
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
                    var captureResponse = _cmtPaymentServiceClient.Post(new CaptureRequest
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

                var response = _cmtPaymentServiceClient.Post(new CaptureRequest
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
                var root = ApplicationPathResolver.GetApplicationPath(RequestContext);
                var response = _cmtMobileServiceClient.Post(new PairingRequest
                {
                    AutoTipAmount = request.AutoTipAmount,
                    AutoTipPercentage = request.AutoTipPercentage,
                    AutoCompletePayment = true,
                    CallbackUrl = new Uri(root + "/api/payments/cmt/callback/" + request.OrderId).AbsoluteUri,
                    CustomerId = orderStatusDetail.AccountId.ToString(),
                    CustomerName = accountDetail.Name,
                    DriverId = orderStatusDetail.DriverInfos.VehicleRegistration,
                    Latitude = orderStatusDetail.VehicleLatitude.GetValueOrDefault(),
                    Longitude = orderStatusDetail.VehicleLongitude.GetValueOrDefault(),
                    Medallion = orderStatusDetail.VehicleNumber
                });

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
                        throw new TimeoutException("Could not be paired with vehicle");
                    }
                }

                // send a command to save the pairing state for this order
                _commandBus.Send(new PairForRideLinqCmtPayment
                {
                    OrderId = request.OrderId,
                    Medallion = response.Medallion,
                    DriverId = response.DriverId.ToString(CultureInfo.InvariantCulture),
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
                var response = _cmtMobileServiceClient.Delete(new UnpairingRequest
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
                if (request == null) throw new Exception("Received callback but couldn't cast to CallbackRequest/Trip");

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
                var trip = _cmtMobileServiceClient.Get(new TripRequest {Token = pairingToken});
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