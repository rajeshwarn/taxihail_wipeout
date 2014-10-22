﻿#region

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments.Authorization;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments.Pair;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments.Reverse;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments.Tokenize;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Api.Contract.Resources.Payments.Cmt;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.Text;

#endregion

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class CmtPaymentService : IPaymentService
    {
        private readonly ICommandBus _commandBus;
        private readonly IOrderDao _orderDao;
        private readonly IAccountDao _accountDao;
        private readonly IServerSettings _serverSettings;
        private readonly IPairingService _pairingService;
        private readonly ILogger _logger;
        private readonly IIbsOrderService _ibs;
        private readonly IOrderPaymentDao _orderPaymentDao;
        private readonly CmtPaymentServiceClient _cmtPaymentServiceClient;
        private readonly CmtMobileServiceClient _cmtMobileServiceClient;

        public CmtPaymentService(ICommandBus commandBus, 
            IOrderDao orderDao,
            ILogger logger, 
            IIbsOrderService ibs,
            IAccountDao accountDao, 
            IOrderPaymentDao orderPaymentDao,
            IServerSettings serverSettings,
            IPairingService pairingService)
        {
            _commandBus = commandBus;
            _orderDao = orderDao;
            _logger = logger;
            _ibs = ibs;
            _accountDao = accountDao;
            _orderPaymentDao = orderPaymentDao;
            _serverSettings = serverSettings;
            _pairingService = pairingService;

            _cmtPaymentServiceClient =
                new CmtPaymentServiceClient(serverSettings.GetPaymentSettings().CmtPaymentSettings, null, null, logger);
            _cmtMobileServiceClient =
                new CmtMobileServiceClient(serverSettings.GetPaymentSettings().CmtPaymentSettings, null,  null);
        }

        private CmtPairingResponse PairWithVehicleUsingRideLinq(OrderStatusDetail orderStatusDetail, PairingForPaymentRequest request)
        {
            var accountDetail = _accountDao.FindById(orderStatusDetail.AccountId);

            // send pairing request                                
            var cmtPaymentSettings = _serverSettings.GetPaymentSettings().CmtPaymentSettings;
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
                CardOnFileId = request.CardToken,
                Market = cmtPaymentSettings.Market
            };

            _logger.LogMessage("Pairing request : " + pairingRequest.ToJson());
            _logger.LogMessage("PaymentSettings request : " + cmtPaymentSettings.ToJson());

            var response = _cmtMobileServiceClient.Post(pairingRequest);

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

            return response;
        }

        private void UnpairFromVehicleUsingRideLinq(OrderPairingDetail orderPairingDetail)
        {
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
        }

        public PairingResponse Pair(PairingForPaymentRequest request)
        {
            try
            {
                if (_serverSettings.GetPaymentSettings().PaymentMode == PaymentMethod.RideLinqCmt)
                {
                    var orderStatusDetail = _orderDao.FindOrderStatusById(request.OrderId);
                    if (orderStatusDetail == null) throw new HttpError(HttpStatusCode.BadRequest, "Order not found");
                    if (orderStatusDetail.IBSOrderId == null)
                        throw new HttpError(HttpStatusCode.BadRequest, "Order has no IBSOrderId");

                    var response = PairWithVehicleUsingRideLinq(orderStatusDetail, request);

                    // send a command to save the pairing state for this order
                    _commandBus.Send(new PairForPayment
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
                        IsSuccessful = true,
                        Message = "Success",
                        PairingToken = response.PairingToken,
                        PairingCode = response.PairingCode,
                        Medallion = response.Medallion,
                        TripId = response.TripId,
                        DriverId = response.DriverId
                    };
                }

                _pairingService.Pair(request.OrderId, request.CardToken, request.AutoTipPercentage);

                return new PairingResponse
                {
                    IsSuccessful = true,
                    Message = "Success"
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e);
                return new PairingResponse
                {
                    IsSuccessful = false,
                    Message = e.Message
                };
            }
        }

        public BasePaymentResponse Unpair(UnpairingForPaymentRequest request)
        {
            try
            {
                if (_serverSettings.GetPaymentSettings().PaymentMode == PaymentMethod.RideLinqCmt)
                {
                    var orderPairingDetail = _orderDao.FindOrderPairingById(request.OrderId);
                    if (orderPairingDetail == null) throw new HttpError(HttpStatusCode.BadRequest, "Order not found");
                    UnpairFromVehicleUsingRideLinq(orderPairingDetail);

                    // send a command to delete the pairing pairing info for this order
                    _commandBus.Send(new UnpairForPayment
                    {
                        OrderId = request.OrderId
                    });
                }

                _pairingService.Unpair(request.OrderId);

                return new BasePaymentResponse
                {
                    IsSuccessful = true,
                    Message = "Success"
                };
            }
            catch (Exception e)
            {
                return new BasePaymentResponse
                {
                    IsSuccessful = false,
                    Message = e.Message
                };
            }
        }

        public void VoidPreAuthorization(Guid orderId)
        {
            // nothing to do for CMT since there's no notion of preauth
        }

        private void Void(string fleetToken, string deviceId, long transactionId, int driverId, int tripId, ref string message)
        {
            var reverseRequest = new ReverseRequest
            {
                FleetToken = fleetToken,
                DeviceId = deviceId,
                TransactionId = transactionId,
                DriverId = driverId,
                TripId = tripId
            };

            var responseReverseTask = _cmtPaymentServiceClient.PostAsync(reverseRequest);
            responseReverseTask.Wait();
            var reverseResponse = responseReverseTask.Result;
            _logger.LogMessage("CMT reverse response : " + reverseResponse.ResponseMessage);

            if (reverseResponse.ResponseCode != 1)
            {
                throw new Exception("Cannot cancel cmt transaction");
            }

            message = message + " The transaction has been cancelled.";
        }

        public DeleteTokenizedCreditcardResponse DeleteTokenizedCreditcard(DeleteTokenizedCreditcardRequest request)
        {
            try
            {
                var responseTask = _cmtPaymentServiceClient.DeleteAsync(new TokenizeDeleteRequest
                {
                    CardToken = request.CardToken
                });
                responseTask.Wait();
                var response = responseTask.Result;

                return new DeleteTokenizedCreditcardResponse
                {
                    IsSuccessful = response.ResponseCode == 1,
                    Message = response.ResponseMessage
                };
            }
            catch (AggregateException ex)
            {
                ex.Handle(x =>
                {
                    _logger.LogError(x);
                    return true;
                });
                return new DeleteTokenizedCreditcardResponse
                {
                    IsSuccessful = false,
                    Message = ex.InnerExceptions.First().Message,
                };
            }
        }

        public PreAuthorizePaymentResponse PreAuthorize(Guid orderId, string email, string cardToken, decimal amountToPreAuthorize)
        {
            return new PreAuthorizePaymentResponse
            {
                IsSuccessful = true,
                Message = string.Empty
            };
        }

        public CommitPreauthorizedPaymentResponse CommitPayment(CommitPaymentRequest request)
        {
            return PreAuthorizeAndCommitPayment(request);
        }

        private CommitPreauthorizedPaymentResponse PreAuthorizeAndCommitPayment(CommitPaymentRequest request)
        {
            string transactionId = null;
            try
            {
                var isSuccessful = false;
                var authorizationCode = string.Empty;
                string message;

                var orderDetail = _orderDao.FindById(request.OrderId);
                if (orderDetail == null) throw new HttpError(HttpStatusCode.BadRequest, "Order not found");
                if (orderDetail.IBSOrderId == null)
                    throw new HttpError(HttpStatusCode.BadRequest, "Order has no IBSOrderId");

                var account = _accountDao.FindById(orderDetail.AccountId);

                //check if already a payment
                if (_orderPaymentDao.FindByOrderId(request.OrderId) != null)
                {
                    return new CommitPreauthorizedPaymentResponse
                    {
                        IsSuccessful = false,
                        Message = "order already paid or payment currently processing"
                    };
                }

                var orderStatus = _orderDao.FindOrderStatusById(orderDetail.Id);
                if (orderStatus == null) throw new HttpError(HttpStatusCode.BadRequest, "Order status not found");

                var deviceId = orderStatus.VehicleNumber;
                var driverId = orderStatus.DriverInfos == null ? 0 : orderStatus.DriverInfos.DriverId.To<int>();
                var employeeId = orderStatus.DriverInfos == null ? "" : orderStatus.DriverInfos.DriverId;
                var tripId = orderStatus.IBSOrderId.Value;
                var fleetToken = _serverSettings.GetPaymentSettings().CmtPaymentSettings.FleetToken;
                var customerReferenceNumber = orderStatus.ReferenceNumber.HasValue() ?
                                                    orderStatus.ReferenceNumber :
                                                    orderDetail.IBSOrderId.ToString();

                var authRequest = new AuthorizationRequest
                {
                    FleetToken = fleetToken,
                    DeviceId = deviceId,
                    Amount = (int)(request.Amount * 100),
                    CardOnFileToken = request.CardToken,
                    CustomerReferenceNumber = customerReferenceNumber,
                    DriverId = driverId,
                    EmployeeId = employeeId,
                    ShiftUuid = orderDetail.Id.ToString(),
                    Fare = (int)(request.MeterAmount * 100),
                    Tip = (int)(request.TipAmount * 100),
                    TripId = tripId,
                    ConvenienceFee = 0,
                    Extras = 0,
                    Surcharge = 0,
                    Tax = 0,
                    Tolls = 0
                };

                var responseTask = _cmtPaymentServiceClient.PostAsync(authRequest);
                responseTask.Wait();
                var authResponse = responseTask.Result;
                message = authResponse.ResponseMessage;
                transactionId = authResponse.TransactionId.ToString(CultureInfo.InvariantCulture);

                if (authResponse.ResponseCode == 1)
                {
                    isSuccessful = true;
                    
                    var paymentId = Guid.NewGuid();

                    _commandBus.Send(new InitiateCreditCardPayment
                    {
                        PaymentId = paymentId,
                        TransactionId = transactionId,
                        Amount = 0,
                        OrderId = request.OrderId,
                        Tip = 0,
                        Meter = 0,
                        CardToken = request.CardToken,
                        Provider = PaymentProvider.Cmt,
                        IsNoShowFee = request.IsNoShowFee
                    });

                    // wait for OrderPaymentDetail to be created
                    Thread.Sleep(500);

                    authorizationCode = authResponse.AuthorizationCode;

                    //send information to IBS
                    if (!request.IsNoShowFee)
                    { 
                        try
                        {
                            _ibs.ConfirmExternalPayment(orderDetail.Id,
                                orderDetail.IBSOrderId.Value,
                                Convert.ToDecimal(request.Amount),
                                Convert.ToDecimal(request.TipAmount),
                                Convert.ToDecimal(request.MeterAmount),
                                PaymentType.CreditCard.ToString(),
                                PaymentProvider.Cmt.ToString(),
                                transactionId,
                                authorizationCode,
                                request.CardToken,
                                account.IBSAccountId,
                                orderDetail.Settings.Name,
                                orderDetail.Settings.Phone,
                                account.Email,
                                orderDetail.UserAgent.GetOperatingSystem(),
                                orderDetail.UserAgent);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e);
                            message = e.Message;
                            isSuccessful = false;

                            //cancel CMT transaction
                            try
                            {
                                Void(fleetToken, deviceId, authResponse.TransactionId, driverId, tripId, ref message);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogMessage("Can't cancel CMT transaction");
                                _logger.LogError(ex);
                                message = message + ex.Message;
                            }
                        }
                    }

                    if (isSuccessful)
                    {
                        //payment completed
                        _commandBus.Send(new CaptureCreditCardPayment
                        {
                            PaymentId = paymentId,
                            AuthorizationCode = authorizationCode,
                            Provider = PaymentProvider.Cmt,
                            Amount = Convert.ToDecimal(request.Amount),
                            MeterAmount = Convert.ToDecimal(request.MeterAmount),
                            TipAmount = Convert.ToDecimal(request.TipAmount),
                            IsNoShowFee = request.IsNoShowFee
                        });
                    }
                    else
                    {
                        //payment failed
                        _commandBus.Send(new LogCreditCardError
                        {
                            PaymentId = paymentId,
                            Reason = message
                        });
                    }
                }

                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessful = isSuccessful,
                    TransactionId = transactionId,
                    Message = message,
                    AuthorizationCode = authorizationCode,
                };
            }
            catch (AggregateException ex)
            {
                ex.Handle(x =>
                {
                    _logger.LogError(x);
                    return true;
                });
                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessful = false,
                    TransactionId = transactionId,
                    Message = ex.InnerExceptions.First().Message,
                };
            }
            catch (Exception e)
            {
                _logger.LogMessage("Error during payment " + e);
                _logger.LogError(e);
                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessful = false,
                    TransactionId = transactionId,
                    Message = e.Message,
                };
            }
        }

        private Trip GetTrip(string pairingToken)
        {
            try
            {
                var trip = _cmtMobileServiceClient.Get(new TripRequest { Token = pairingToken });
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