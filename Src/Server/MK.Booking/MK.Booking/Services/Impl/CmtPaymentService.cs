using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Resources;
using CMTPayment;
using CMTPayment.Authorization;
using CMTPayment.Pair;
using CMTPayment.Reverse;
using CMTPayment.Tokenize;
using Infrastructure.Messaging;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Services.Impl
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

            _cmtPaymentServiceClient = new CmtPaymentServiceClient(serverSettings.GetPaymentSettings().CmtPaymentSettings, null, null, logger);
            _cmtMobileServiceClient = new CmtMobileServiceClient(serverSettings.GetPaymentSettings().CmtPaymentSettings, null, null);
        }
        
        public PairingResponse Pair(Guid orderId, string cardToken, int? autoTipPercentage, double? autoTipAmount)
        {
            try
            {
                if (_serverSettings.GetPaymentSettings().PaymentMode == PaymentMethod.RideLinqCmt)
                {
                    var orderStatusDetail = _orderDao.FindOrderStatusById(orderId);
                    if (orderStatusDetail == null)
                    {
                        throw new Exception("Order not found");
                    }

                    if (orderStatusDetail.IBSOrderId == null)
                    {
                        throw new Exception("Order has no IBSOrderId");
                    }

                    var response = PairWithVehicleUsingRideLinq(orderStatusDetail, orderId, cardToken, autoTipPercentage, autoTipAmount);

                    // send a command to save the pairing state for this order
                    _commandBus.Send(new PairForPayment
                    {
                        OrderId = orderId,
                        Medallion = response.Medallion,
                        DriverId = response.DriverId.ToString(),
                        PairingToken = response.PairingToken,
                        PairingCode = response.PairingCode,
                        TokenOfCardToBeUsedForPayment = cardToken,
                        AutoTipAmount = autoTipAmount,
                        AutoTipPercentage = autoTipPercentage
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

                _pairingService.Pair(orderId, cardToken, autoTipPercentage);

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

        public BasePaymentResponse Unpair(Guid orderId)
        {
            try
            {
                if (_serverSettings.GetPaymentSettings().PaymentMode == PaymentMethod.RideLinqCmt)
                {
                    var orderPairingDetail = _orderDao.FindOrderPairingById(orderId);
                    if (orderPairingDetail == null)
                    {
                        throw new Exception("Order not found");
                    }

                    UnpairFromVehicleUsingRideLinq(orderPairingDetail);

                    // send a command to delete the pairing pairing info for this order
                    _commandBus.Send(new UnpairForPayment
                    {
                        OrderId = orderId
                    });
                }

                _pairingService.Unpair(orderId);

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

        public DeleteTokenizedCreditcardResponse DeleteTokenizedCreditcard(string cardToken)
        {
            try
            {
                var responseTask = _cmtPaymentServiceClient.DeleteAsync(new TokenizeDeleteRequest
                {
                    CardToken = cardToken
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

        public CommitPreauthorizedPaymentResponse CommitPayment(decimal amount, decimal meterAmount, decimal tipAmount, string cardToken, Guid orderId, bool isNoShowFee)
        {
            return PreAuthorizeAndCommitPayment(amount, meterAmount, tipAmount, cardToken, orderId, isNoShowFee);
        }

        private CommitPreauthorizedPaymentResponse PreAuthorizeAndCommitPayment(decimal amount, decimal meterAmount, decimal tipAmount, string cardToken, Guid orderId, bool isNoShowFee)
        {
            string transactionId = null;
            try
            {
                var isSuccessful = false;
                var authorizationCode = string.Empty;
                string message;

                var orderDetail = _orderDao.FindById(orderId);
                if (orderDetail == null)
                {
                    throw new Exception("Order not found");
                }

                if (orderDetail.IBSOrderId == null)
                {
                    throw new Exception("Order has no IBSOrderId");
                }

                var account = _accountDao.FindById(orderDetail.AccountId);

                //check if already a payment
                if (_orderPaymentDao.FindNonPayPalByOrderId(orderId) != null)
                {
                    return new CommitPreauthorizedPaymentResponse
                    {
                        IsSuccessful = false,
                        Message = "order already paid or payment currently processing"
                    };
                }

                var orderStatus = _orderDao.FindOrderStatusById(orderDetail.Id);
                if (orderStatus == null)
                {
                    throw new Exception("Order status not found");
                }

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
                    Amount = (int)(amount * 100),
                    CardOnFileToken = cardToken,
                    CustomerReferenceNumber = customerReferenceNumber,
                    DriverId = driverId,
                    EmployeeId = employeeId,
                    ShiftUuid = orderDetail.Id.ToString(),
                    Fare = (int)(meterAmount * 100),
                    Tip = (int)(tipAmount * 100),
                    TripId = tripId,
                    ConvenienceFee = 0,
                    Extras = 0,
                    Surcharge = 0,
                    Tax = 0,
                    Tolls = 0
                };

                AuthorizationResponse authResponse = new AuthorizationResponse();
                try
                {
                    var responseTask = _cmtPaymentServiceClient.PostAsync(authRequest);
                    responseTask.Wait();
                    authResponse = responseTask.Result;                    
                }
                catch
                {
                    SendDeclineToIbs(amount, meterAmount, tipAmount, cardToken, orderId, isNoShowFee, transactionId, authorizationCode, orderDetail, account);
                    throw;
                }


                message = authResponse.ResponseMessage;
                transactionId = authResponse.TransactionId.ToString(CultureInfo.InvariantCulture);
                var paymentId = Guid.NewGuid();
                _commandBus.Send(new InitiateCreditCardPayment
                {
                    PaymentId = paymentId,
                    TransactionId = transactionId,
                    Amount = 0,
                    OrderId = orderId,
                    Tip = 0,
                    Meter = 0,
                    CardToken = cardToken,
                    Provider = PaymentProvider.Cmt,
                    IsNoShowFee = isNoShowFee
                });
                
                if (authResponse.ResponseCode == 1)
                {
                    isSuccessful = true;
                                        
                    // wait for OrderPaymentDetail to be created
                    Thread.Sleep(500);

                    authorizationCode = authResponse.AuthorizationCode;

                    //send information to IBS
                    if (!isNoShowFee)
                    { 
                        try
                        {
                            _ibs.ConfirmExternalPayment(orderDetail.Id,
                                orderDetail.IBSOrderId.Value,
                                Convert.ToDecimal(amount),
                                Convert.ToDecimal(tipAmount),
                                Convert.ToDecimal(meterAmount),
                                PaymentType.CreditCard.ToString(),
                                PaymentProvider.Cmt.ToString(),
                                transactionId,
                                authorizationCode,
                                cardToken,
                                account.IBSAccountId.Value,
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
                            Amount = Convert.ToDecimal(amount),
                            MeterAmount = Convert.ToDecimal(meterAmount),
                            TipAmount = Convert.ToDecimal(tipAmount),
                            IsNoShowFee = isNoShowFee
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

        private void SendDeclineToIbs(decimal amount, decimal meterAmount, decimal tipAmount, string cardToken, Guid orderId, bool isNoShowFee, string transactionId, string authorizationCode, ReadModel.OrderDetail orderDetail, ReadModel.AccountDetail account)
        {
            try
            {
                _logger.LogMessage("Sending invalid payment info with payment type 0");
                _ibs.ConfirmExternalPayment(orderDetail.Id,
                           orderDetail.IBSOrderId.Value,
                           Convert.ToDecimal(amount),
                           Convert.ToDecimal(tipAmount),
                           Convert.ToDecimal(meterAmount),
                           "0",
                           PaymentProvider.Cmt.ToString(),
                           transactionId,
                           authorizationCode,
                           cardToken,
                           account.IBSAccountId.Value,
                           orderDetail.Settings.Name,
                           orderDetail.Settings.Phone,
                           account.Email,
                           orderDetail.UserAgent.GetOperatingSystem(),
                           orderDetail.UserAgent);

                _commandBus.Send(new InitiateCreditCardPayment
                {
                    PaymentId = Guid.NewGuid(),
                    TransactionId = transactionId,
                    Amount = amount,
                    OrderId = orderId,
                    Tip = tipAmount,
                    Meter = meterAmount,
                    CardToken = cardToken,
                    Provider = PaymentProvider.Cmt,
                    IsNoShowFee = isNoShowFee

                });
            }
            catch
            {

            }
        }

        private CmtPairingResponse PairWithVehicleUsingRideLinq(OrderStatusDetail orderStatusDetail, Guid orderId, string cardToken, int? autoTipPercentage, double? autoTipAmount)
        {
            var accountDetail = _accountDao.FindById(orderStatusDetail.AccountId);

            // send pairing request                                
            var cmtPaymentSettings = _serverSettings.GetPaymentSettings().CmtPaymentSettings;
            var pairingRequest = new PairingRequest
            {
                AutoTipAmount = autoTipAmount,
                AutoTipPercentage = autoTipPercentage,
                AutoCompletePayment = true,
                CallbackUrl = "",
                CustomerId = orderStatusDetail.IBSOrderId.ToString(),
                CustomerName = accountDetail.Name,
                DriverId = orderStatusDetail.DriverInfos.DriverId,
                Latitude = orderStatusDetail.VehicleLatitude.GetValueOrDefault(),
                Longitude = orderStatusDetail.VehicleLongitude.GetValueOrDefault(),
                Medallion = orderStatusDetail.VehicleNumber,
                CardOnFileId = cardToken,
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