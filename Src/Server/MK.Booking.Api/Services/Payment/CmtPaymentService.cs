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
using apcurium.MK.Booking.Api.Contract.Requests.Payment.Cmt;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Api.Contract.Resources.Payments.Cmt;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.Text;

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
        private readonly IIbsOrderService _ibs;
        private readonly IOrderPaymentDao _ordrPaymentDao;

        public CmtPaymentService(ICommandBus commandBus, IOrderDao orderDao,
            IAccountDao accountDao, IConfigurationManager configurationManager, ILogger logger, IIbsOrderService ibs, IOrderPaymentDao ordrPaymentDao)
        {
            _commandBus = commandBus;
            _orderDao = orderDao;
            _accountDao = accountDao;

            _configurationManager = configurationManager;
            _logger = logger;
            _ibs = ibs;
            _ordrPaymentDao = ordrPaymentDao;
            _cmtPaymentServiceClient =
                new CmtPaymentServiceClient(configurationManager.GetPaymentSettings().CmtPaymentSettings, null,
                    "TaxiHail");
            _cmtMobileServiceClient =
                new CmtMobileServiceClient(configurationManager.GetPaymentSettings().CmtPaymentSettings, null,
                    "TaxiHail");
        }

        public DeleteTokenizedCreditcardResponse Delete(DeleteTokenizedCreditcardCmtRequest request)
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
                    IsSuccessfull = response.ResponseCode == 1,
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
                    IsSuccessfull = false,
                    Message = ex.InnerExceptions.First().Message,
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

                var session = this.GetSession();
                var account = _accountDao.FindById(new Guid(session.UserAuthId));

                var orderDetail = _orderDao.FindById(request.OrderId);
                if (orderDetail == null) throw new HttpError(HttpStatusCode.BadRequest, "Order not found");
                if (orderDetail.IBSOrderId == null)
                    throw new HttpError(HttpStatusCode.BadRequest, "Order has no IBSOrderId");

                //check if already a payment
                if (_ordrPaymentDao.FindByOrderId(request.OrderId) != null)
                {
                    return new CommitPreauthorizedPaymentResponse
                    {
                        IsSuccessfull = false,
                        Message = "order already paid or payment currently processing"
                    };
                }

                var orderStatus = _orderDao.FindOrderStatusById(orderDetail.Id);
                if (orderStatus == null) throw new HttpError(HttpStatusCode.BadRequest, "Order status not found");

                // TODO verify these!!
                var deviceId = orderStatus.VehicleNumber; //? previously DeviceName = orderStatus.TerminalId
                var driverId = orderStatus.DriverInfos == null ? 0 : orderStatus.DriverInfos.DriverId.To<int>(); //?
                var employeeId = orderStatus.DriverInfos == null ? "" : orderStatus.DriverInfos.DriverId; //?
                var tripId = orderStatus.IBSOrderId.Value; //?

                var authRequest = new AuthorizationRequest
                {
                    FleetToken = _configurationManager.GetPaymentSettings().CmtPaymentSettings.FleetToken,
                    DeviceId = deviceId,

                    Amount = (int) (request.Amount*100),
                    CardOnFileToken = request.CardToken,              
                    CustomerReferenceNumber = string.IsNullOrEmpty(orderStatus.ReferenceNumber) ? orderDetail.IBSOrderId.ToString() : orderStatus.ReferenceNumber,
                    DriverId = driverId,
                    EmployeeId = employeeId,
                    Fare =  (int) (request.MeterAmount*100),
                    Tip = (int) (request.TipAmount*100),
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

                if (authResponse.ResponseCode == 1)
                {
                    isSuccessful = true;
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
                        Provider = PaymentProvider.Cmt,
                    });

                    // wait for OrderPaymentDetail to be created
                    Thread.Sleep(500);

                    authorizationCode = authResponse.AuthorizationCode;

                    //send information to IBS
                    try
                    {
                        _ibs.ConfirmExternalPayment(orderDetail.IBSOrderId.Value,
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
                            var reverseRequest = new ReverseRequest
                            {
                                FleetToken = _configurationManager.GetPaymentSettings().CmtPaymentSettings.FleetToken,
                                DeviceId = deviceId,

                                TransactionId = authResponse.TransactionId,
                                DriverId = driverId,
                                TripId = tripId
                            };

                            var responseReverseTask = _cmtPaymentServiceClient.PostAsync(reverseRequest);
                            responseReverseTask.Wait();
                            var reverseResponse = responseReverseTask.Result;
                            message = reverseResponse.ResponseMessage;

                            if (reverseResponse.ResponseCode != 1)
                            {
                                throw new Exception("Cannot cancel cmt transaction");
                            }

							message = message + " the transaction has been cancelled.";
                        }
                        catch (Exception ex)
                        {
                            _logger.LogMessage("can't cancel cmt transaction");
                            _logger.LogError(ex);
                            message = message + ex.Message;
                            //can't cancel transaction, send a command to log
                            _commandBus.Send(new LogCreditCardPaymentCancellationFailed
                            {
                                PaymentId = paymentId,
                                Reason = message
                            });
                        }
                    }

                    //payment completed
                    _commandBus.Send(new CaptureCreditCardPayment
                    {
                        PaymentId = paymentId,
                        AuthorizationCode = authorizationCode,
						Provider = PaymentProvider.Cmt,
                    });
                }

                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessfull = isSuccessful,
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
                    IsSuccessfull = false,
                    Message = ex.InnerExceptions.First().Message,
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


                _logger.LogMessage("Pairing request : " + pairingRequest.ToJson());
                _logger.LogMessage("PaymentSettings request : " + _configurationManager.GetPaymentSettings().CmtPaymentSettings.ToJson());


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
                _logger.LogError(e);
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