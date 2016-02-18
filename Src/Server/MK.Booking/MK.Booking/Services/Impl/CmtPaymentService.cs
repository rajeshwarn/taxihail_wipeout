using System;
using System.Globalization;
using System.Linq;
using System.Net;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
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
using Newtonsoft.Json;
using CMTPayment.Actions;
using MK.Common.Exceptions;

namespace apcurium.MK.Booking.Services.Impl
{
    public class CmtPaymentService : IPaymentService
    {
        private readonly ICommandBus _commandBus;
        private readonly IOrderDao _orderDao;
        private readonly IAccountDao _accountDao;
        private readonly ServerPaymentSettings _serverPaymentSettings;
        private readonly IPairingService _pairingService;
        private readonly ICreditCardDao _creditCardDao;
        private readonly ILogger _logger;
        private readonly IOrderPaymentDao _paymentDao;

        private CmtPaymentServiceClient _cmtPaymentServiceClient;
        private CmtMobileServiceClient _cmtMobileServiceClient;
        private CmtTripInfoServiceHelper _cmtTripInfoServiceHelper;

        public CmtPaymentService(ICommandBus commandBus, 
            IOrderDao orderDao,
            ILogger logger, 
            IAccountDao accountDao, 
            IOrderPaymentDao paymentDao,
            ServerPaymentSettings serverPaymentSettings,
            IPairingService pairingService,
            ICreditCardDao creditCardDao)
        {
            _commandBus = commandBus;
            _orderDao = orderDao;
            _logger = logger;
            _accountDao = accountDao;
            _paymentDao = paymentDao;
            _serverPaymentSettings = serverPaymentSettings;
            _pairingService = pairingService;
            _creditCardDao = creditCardDao;
        }

        public PaymentProvider ProviderType(string companyKey, Guid? orderId = null)
        {
            return PaymentProvider.Cmt;
        }

        public bool IsPayPal(Guid? accountId = null, Guid? orderId = null, bool isForPrepaid = false)
        {
            return false;
        }
        
        public PairingResponse Pair(string companyKey, Guid orderId, string cardToken, int autoTipPercentage)
        {
            try
            {
                if (_serverPaymentSettings.PaymentMode == PaymentMethod.RideLinqCmt)
                {
                    // CMT RideLinq flow
                   
                    var orderStatusDetail = _orderDao.FindOrderStatusById(orderId);
                    if (orderStatusDetail == null)
                    {
                        throw new Exception("Order not found");
                    }

                    var pairingMethod = _serverPaymentSettings.CmtPaymentSettings.PairingMethod;
                    if (pairingMethod == RideLinqPairingMethod.NotSet && companyKey.HasValueTrimmed())
                    {
                        // we are on a company that wasn't migrated to the new pairing method setting, use the old one
                        pairingMethod = _serverPaymentSettings.CmtPaymentSettings.UsePairingCode
                            ? RideLinqPairingMethod.PairingCode
                            : RideLinqPairingMethod.VehicleMedallion;
                    }

                    if (pairingMethod == RideLinqPairingMethod.PairingCode
                        && !orderStatusDetail.RideLinqPairingCode.HasValue())
                    {
                        // We haven't received the pairing code from IBS yet, set the ignore response false
                        // so that the caller can exit without interpreting the response as a failure
                        return new PairingResponse { IgnoreResponse = true };
                    }

                    _logger.LogMessage("Starting pairing with RideLinq for Order {0}", orderId);

                    if (orderStatusDetail.IBSOrderId == null)
                    {
                        throw new Exception("Order has no IBSOrderId");
                    }

                    var response = PairWithVehicleUsingRideLinq(pairingMethod, orderStatusDetail, cardToken, autoTipPercentage);

                    if (response.ErrorCode.HasValue)
                    {
                        return new PairingResponse
                        {
                            IsSuccessful = false,
                            ErrorCode = response.ErrorCode
                        };
                    }

                    // send a command to save the pairing state for this order
                    _commandBus.Send(new PairForPayment
                    {
                        OrderId = orderId,
                        Medallion = response.Medallion,
                        DriverId = response.DriverId.ToString(),
                        PairingToken = response.PairingToken,
                        PairingCode = response.PairingCode,
                        TokenOfCardToBeUsedForPayment = cardToken,
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
                else
                {
                    // Normal CMT flow
                    _pairingService.Pair(orderId, cardToken, autoTipPercentage);

                    return new PairingResponse
                    {
                        IsSuccessful = true,
                        Message = "Success"
                    };
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e);

                return new PairingResponse
                {
                    IsSuccessful = false,
                    Message = e.Message,
                    ErrorCode = CmtErrorCodes.UnableToPair
                };
            }
        }

        public BasePaymentResponse Unpair(string companyKey, Guid orderId)
        {
            try
            {
                if (_serverPaymentSettings.PaymentMode == PaymentMethod.RideLinqCmt)
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

        public void VoidPreAuthorization(string companyKey, Guid orderId, bool isForPrepaid = false)
        {
            // Nothing to do for CMT since there's no notion of preauth
        }

        public void VoidTransaction(string companyKey, Guid orderId, string transactionId, ref string message)
        {
            var orderStatus = _orderDao.FindOrderStatusById(orderId);
            if (orderStatus == null)
            {
                throw new Exception("Order status not found");
            }

            Void(_serverPaymentSettings.CmtPaymentSettings.FleetToken,
                orderStatus.VehicleNumber,
                long.Parse(transactionId),
                orderStatus.DriverInfos == null 
                    ? 0 
                    : orderStatus.DriverInfos.DriverId.ToInt(),
                orderStatus.IBSOrderId.Value, ref message);
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

            var reverseResponse = Reverse(reverseRequest);
            
            if (reverseResponse.ResponseCode != 1)
            {
                throw new Exception("Cannot cancel cmt transaction");
            }

            message = message + " The transaction has been cancelled.";
        }
        
        public DeleteTokenizedCreditcardResponse DeleteTokenizedCreditcard(string cardToken)
        {
            var request = new TokenizeDeleteRequest
            {
                CardToken = cardToken
            };
            var response = DeleteCreditCard(request);

            return new DeleteTokenizedCreditcardResponse
            {
                IsSuccessful = response.ResponseCode == 1,
                Message = response.ResponseMessage
            }; 
        }

        public PreAuthorizePaymentResponse PreAuthorize(string companyKey, Guid orderId, AccountDetail account, decimal amountToPreAuthorize, bool isReAuth = false, bool isSettlingOverduePayment = false, bool isForPrepaid = false, string cvv = null)
        {
            var paymentId = Guid.NewGuid();
            var creditCard = _creditCardDao.FindById(account.DefaultCreditCard.GetValueOrDefault());

            _commandBus.Send(new InitiateCreditCardPayment
            {
                PaymentId = paymentId,
                Amount = 0,
                TransactionId = string.Empty,
                OrderId = orderId,
                CardToken = creditCard.Token,
                Provider = PaymentProvider.Cmt,
                IsNoShowFee = false,
                CompanyKey = companyKey
            });

            return new PreAuthorizePaymentResponse
            {
                IsSuccessful = true,
                Message = string.Empty
            };
        }

        public CommitPreauthorizedPaymentResponse CommitPayment(string companyKey, Guid orderId, AccountDetail account, decimal preauthAmount, decimal amount, decimal meterAmount, decimal tipAmount, 
            string transactionId, string reAuthOrderId = null, bool isForPrepaid = false)
        {
            // No need to use preauthAmount for CMT because we can't preauthorize

            try
            {
                string authorizationCode = null;
                string commitTransactionId = transactionId;

                var orderDetail = _orderDao.FindById(orderId);
                if (orderDetail == null)
                {
                    throw new Exception("Order not found");
                }

                var orderStatus = _orderDao.FindOrderStatusById(orderId);
                if (orderStatus == null)
                {
                    throw new Exception("Order status not found");
                }

                var orderPayment = _paymentDao.FindByOrderId(orderId, companyKey);
                if (orderPayment == null)
                {
                    throw new Exception("Order payment not found");
                }

                var deviceId = orderStatus.VehicleNumber;
                var driverId = orderStatus.DriverInfos == null ? 0 : orderStatus.DriverInfos.DriverId.ToInt();
                var employeeId = orderStatus.DriverInfos == null ? string.Empty : orderStatus.DriverInfos.DriverId;
                var tripId = orderStatus.IBSOrderId.Value;
                var fleetToken = _serverPaymentSettings.CmtPaymentSettings.FleetToken;
                var customerReferenceNumber = orderStatus.ReferenceNumber.HasValue() ?
                                                    orderStatus.ReferenceNumber :
                                                    orderDetail.IBSOrderId.ToString();

                var tempPaymentInfo = _orderDao.GetTemporaryPaymentInfo(orderId);
                var cvv = tempPaymentInfo != null ? tempPaymentInfo.Cvv : null;

                var authRequest = new AuthorizationRequest
                {
                    FleetToken = fleetToken,
                    DeviceId = deviceId,
                    Amount = (int)(amount * 100),
                    CardOnFileToken = orderPayment.CardToken,
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
                    Tolls = 0,
                    Cvv2 = cvv
                };

                // remove temp payment info
                _orderDao.DeleteTemporaryPaymentInfo(orderId);

                var authResponse = Authorize(authRequest);

                var isSuccessful = authResponse.ResponseCode == 1;
                var isCardDeclined = authResponse.ResponseCode == 607;

                if (isSuccessful)
                {
                    commitTransactionId = authResponse.TransactionId.ToString(CultureInfo.InvariantCulture);
                    authorizationCode = authResponse.AuthorizationCode;
                }

                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessful = isSuccessful,
                    AuthorizationCode = authorizationCode,
                    Message = authResponse.ResponseMessage,
                    TransactionId = commitTransactionId,
                    IsDeclined = isCardDeclined,
                    TransactionDate = isSuccessful ? (DateTime?)authResponse.AuthorizationDate : null
                };
            }
            catch (Exception ex)
            {
                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessful = false,
                    TransactionId = transactionId,
                    Message = ex.Message
                };
            }
        }

        public RefundPaymentResponse RefundPayment(string companyKey, Guid orderId)
        {
            if (_serverPaymentSettings.PaymentMode != PaymentMethod.RideLinqCmt)
            {
                throw new Exception("This method can only be used with CMTRideLinQ as a payment provider.");
            }

            InitializeServiceClient();

            try
            {
                var order = _orderDao.FindById(orderId);
                var orderPairing = _orderDao.FindOrderPairingById(orderId);

                if (orderPairing == null)
                {
                    throw new Exception(string.Format("can't find orderPairing object for orderId {0}", orderId));
                }

                var creditCardDetail = _creditCardDao.FindByToken(orderPairing.TokenOfCardToBeUsedForPayment);

                var totalAmount = Convert.ToInt32((order.Fare.GetValueOrDefault()
                                    + order.Tax.GetValueOrDefault()
                                    + order.Toll.GetValueOrDefault()
                                    + order.Tip.GetValueOrDefault()
                                    + order.Surcharge.GetValueOrDefault()) * 100);

                var request = new CmtRideLinqRefundRequest
                {
                    PairingToken = orderPairing.PairingToken,
                    CofToken = orderPairing.TokenOfCardToBeUsedForPayment,
                    LastFour = creditCardDetail != null ? creditCardDetail.Last4Digits : string.Empty,
                    AuthAmount = totalAmount
                };

                _logger.LogMessage("Refunding CMT RideLinq. Request: {0}", request.ToJson());

                var response = _cmtMobileServiceClient.Post(string.Format("payment/{0}/credit", orderPairing.PairingToken), request);

                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    // send a command to update refund status of Order
                    _commandBus.Send(new UpdateRefundedOrder
                    {
                        OrderId = orderId,
                        IsSuccessful = true
                    });

                    return new RefundPaymentResponse
                    {
                        IsSuccessful = true,
                        Last4Digits = creditCardDetail != null ? creditCardDetail.Last4Digits : string.Empty,
                    };
                }
                else
                {
                    return new RefundPaymentResponse
                    {
                        IsSuccessful = false,
                        Last4Digits = creditCardDetail != null ? creditCardDetail.Last4Digits : string.Empty,
                        Message = response != null ? response.StatusDescription : string.Empty
                    };
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                var responseBody = string.Empty;

                if (ex is WebServiceException)
                {
                    responseBody = " [" + ((WebServiceException)ex).ResponseBody + "]";
                }

                _logger.LogMessage("Error when trying to refund CMT RideLinq auto tip" + responseBody);
                message += responseBody;

                _logger.LogError(ex);

                return new RefundPaymentResponse
                {
                    IsSuccessful = false,
                    Last4Digits = string.Empty,
                    Message = message
                };
            }
        }

        public BasePaymentResponse UpdateAutoTip(string companyKey, Guid orderId, int autoTipPercentage)
        {
            if (_serverPaymentSettings.PaymentMode != PaymentMethod.RideLinqCmt)
            {
                throw new Exception("This method can only be used with CMTRideLinQ as a payment provider.");
            }

            InitializeServiceClient();

            try
            {
                var orderDetail = _orderDao.FindById(orderId);
                var accountDetail = _accountDao.FindById(orderDetail.AccountId);
                var orderPairing = _orderDao.FindOrderPairingById(orderId);

                var request = new ManualRideLinqPairingRequest
                {
                    AutoTipPercentage = autoTipPercentage,
                    CustomerId = accountDetail.Id.ToString(),
                    CustomerName = accountDetail.Name,
                    Latitude = orderDetail.PickupAddress.Latitude,
                    Longitude = orderDetail.PickupAddress.Longitude,
                    AutoCompletePayment = true
                };

                _logger.LogMessage("Updating CMT RideLinq auto tip. Request: {0}", request.ToJson());

                var response = _cmtMobileServiceClient.Put(string.Format("init/pairing/{0}", orderPairing.PairingToken), request);

                // Wait for trip to be updated
                _cmtTripInfoServiceHelper.WaitForTipUpdated(orderPairing.PairingToken, autoTipPercentage, response.TimeoutSeconds);

                return new BasePaymentResponse
                {
                    IsSuccessful = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogMessage("Error when trying to update CMT RideLinq auto tip");
                _logger.LogError(ex);

                return new BasePaymentResponse
                {
                    IsSuccessful = false,
                    Message = ex.Message
                };
            }
        }

        private CmtPairingResponse PairWithVehicleUsingRideLinq(RideLinqPairingMethod pairingMethod, OrderStatusDetail orderStatusDetail, string cardToken, int autoTipPercentage)
        {
            InitializeServiceClient();

            try
            {
                var accountDetail = _accountDao.FindById(orderStatusDetail.AccountId);
                var creditCardDetail = _creditCardDao.FindByToken(cardToken);
                var orderDetail = _orderDao.FindById(orderStatusDetail.OrderId);

                // send pairing request                                
                var cmtPaymentSettings = _serverPaymentSettings.CmtPaymentSettings;
                var pairingRequest = new ManualRideLinqCoFPairingRequest
                {
                    AutoTipPercentage = autoTipPercentage,
                    AutoCompletePayment = true,
                    CallbackUrl = string.Empty,
                    CustomerId = orderStatusDetail.AccountId.ToString(),
                    CustomerName = accountDetail.Name,
                    DriverId = orderStatusDetail.DriverInfos.DriverId,
                    Latitude = orderStatusDetail.VehicleLatitude.GetValueOrDefault(),
                    Longitude = orderStatusDetail.VehicleLongitude.GetValueOrDefault(),
                    CardOnFileId = cardToken,
                    Market = cmtPaymentSettings.Market,
                    TripRequestNumber = orderStatusDetail.IBSOrderId.GetValueOrDefault().ToString(),
                    LastFour = creditCardDetail.Last4Digits,
                    TipIncentive = orderDetail.TipIncentive,
                    ZipCode = creditCardDetail.ZipCode,
                    Email = accountDetail.Email,
                    CustomerIpAddress = orderDetail.OriginatingIpAddress,
                    BillingFullName = creditCardDetail.NameOnCard,
                    SessionId = orderDetail.KountSessionId
                };

                switch (pairingMethod)
                {
                    case RideLinqPairingMethod.VehicleMedallion:
                        _logger.LogMessage("OrderPairingManager RideLinq with VehicleMedallion : " + (orderStatusDetail.VehicleNumber.HasValue() ? orderStatusDetail.VehicleNumber : "No vehicle number"));
                        pairingRequest.Medallion = orderStatusDetail.VehicleNumber;
                        break;
                    case RideLinqPairingMethod.PairingCode:
                        _logger.LogMessage("OrderPairingManager RideLinq with PairingCode : " + (orderStatusDetail.RideLinqPairingCode.HasValue() ? orderStatusDetail.RideLinqPairingCode : "No code"));
                        pairingRequest.PairingCode = orderStatusDetail.RideLinqPairingCode;
                        break;
                    case RideLinqPairingMethod.DeviceName:
                        throw new Exception("RideLinq PairingMethod DeviceName not supported on non Arro Servers.  Since we do not do the dispatcher, we have no idea of the device name and can't pair using this.");
                    default:
                        throw new Exception("CmtPaymentSetting.PairingMethod not set and trying to use RideLinq pairing.");
                }

                _logger.LogMessage("Pairing request : " + pairingRequest.ToJson());
                _logger.LogMessage("PaymentSettings request : " + cmtPaymentSettings.ToJson());

                var response = _cmtMobileServiceClient.Post(pairingRequest);

                _logger.LogMessage("Pairing response : " + response.ToJson());

                // Wait for trip to be updated to check if pairing was successful
                var trip = _cmtTripInfoServiceHelper.WaitForTripInfo(response.PairingToken, response.TimeoutSeconds);

                response.ErrorCode = trip != null ? trip.ErrorCode : CmtErrorCodes.UnableToPair;

                return response;
            }
            catch (Exception ex)
            {
                var aggregateException = ex as AggregateException;
                if (aggregateException == null)
                {
                    throw;
                }

                var webServiceException = aggregateException.InnerException as WebServiceException;
                if (webServiceException == null)
                {
                    throw;
                }

                var response = JsonConvert.DeserializeObject<AuthorizationResponse>(webServiceException.ResponseBody);

                _logger.LogMessage(string.Format("Error when trying to pair using RideLinQ. Code: {0} - {1}"), response.ResponseCode, response.ResponseMessage);

                throw;
            }
        }

        private void UnpairFromVehicleUsingRideLinq(OrderPairingDetail orderPairingDetail)
        {
            InitializeServiceClient();

            // send unpairing request
            var response = _cmtMobileServiceClient.Delete(new UnpairingRequest
            {
                PairingToken = orderPairingDetail.PairingToken
            });

            // wait for trip to be updated
            _cmtTripInfoServiceHelper.WaitForRideLinqUnpaired(orderPairingDetail.PairingToken, response.TimeoutSeconds);
        }

        private AuthorizationResponse Authorize(AuthorizationRequest request)
        {
            InitializeServiceClient();

            MerchantAuthorizationRequest merchantRequest = null;


            if (!_serverPaymentSettings.CmtPaymentSettings.SubmitAsFleetAuthorization)
            {
                merchantRequest = new MerchantAuthorizationRequest(request)
                {
                    MerchantToken = _serverPaymentSettings.CmtPaymentSettings.MerchantToken
                };
            }

            AuthorizationResponse response;

            try
            {
                var responseTask = merchantRequest != null
                    ? _cmtPaymentServiceClient.PostAsync(merchantRequest)
                    : _cmtPaymentServiceClient.PostAsync(request);

                responseTask.Wait();
                response = responseTask.Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);

                var aggregateException = ex as AggregateException;
                if (aggregateException == null)
                {
                    throw ex;
                }

                var webServiceException = aggregateException.InnerException as WebServiceException;
                if (webServiceException == null)
                {
                    throw ex;
                }

                response = JsonConvert.DeserializeObject<AuthorizationResponse>(webServiceException.ResponseBody);
            }

            return response;
        }

        private TokenizeDeleteResponse DeleteCreditCard(TokenizeDeleteRequest request)
        {
            return new TokenizeDeleteResponse
            {
                ResponseCode = 1
            };
        }

        private ReverseResponse Reverse(ReverseRequest request)
        {
            InitializeServiceClient();

            ReverseResponse response;

            try
            {
                var responseReverseTask = _cmtPaymentServiceClient.PostAsync(request);
                responseReverseTask.Wait();
                response = responseReverseTask.Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);

                var aggregateException = ex as AggregateException;
                if (aggregateException == null)
                {
                    throw ex;
                }

                var webServiceException = aggregateException.InnerException as WebServiceException;
                if (webServiceException == null)
                {
                    throw ex;
                }

                response = JsonConvert.DeserializeObject<ReverseResponse>(webServiceException.ResponseBody);
            }
            
            _logger.LogMessage("CMT reverse response : " + response.ResponseMessage);

            return response;
        }

        private void InitializeServiceClient()
        {
            _cmtPaymentServiceClient = new CmtPaymentServiceClient(_serverPaymentSettings.CmtPaymentSettings, null, null, _logger, null);
            _cmtMobileServiceClient = new CmtMobileServiceClient(_serverPaymentSettings.CmtPaymentSettings, null, null, null);
            _cmtTripInfoServiceHelper = new CmtTripInfoServiceHelper(_cmtMobileServiceClient, _logger);
        }
    }
}