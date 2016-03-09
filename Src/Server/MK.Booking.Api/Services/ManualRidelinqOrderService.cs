using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Resources;
using CMTPayment;
using CMTPayment.Pair;
using Infrastructure.Messaging;
using ManualRideLinqPairingRequest = apcurium.MK.Booking.Api.Contract.Requests.Payment.ManualRideLinqPairingRequest;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Extensions;
using MK.Common.Exceptions;

namespace apcurium.MK.Booking.Api.Services
{
    public class ManualRidelinqOrderService : BaseApiService
    {
        private readonly IOrderDao _orderDao;
        private readonly IAccountDao _accountDao;
        private readonly ICreditCardDao _creditCardDao;
        private readonly ICommandBus _commandBus;
        private readonly IServerSettings _serverSettings;
        private readonly ILogger _logger;
        private readonly CmtMobileServiceClient _cmtMobileServiceClient;
        private readonly CmtTripInfoServiceHelper _cmtTripInfoServiceHelper;
        private readonly Resources.Resources _resources;
		private readonly INotificationService _notificationService;

        public ManualRidelinqOrderService(
            ICommandBus commandBus,
            IOrderDao orderDao,
            IAccountDao accountDao,
            ICreditCardDao creditCardDao,
            IServerSettings serverSettings,
            ILogger logger,
			INotificationService notificationService)
        {
            _commandBus = commandBus;
            _orderDao = orderDao;
            _accountDao = accountDao;
            _creditCardDao = creditCardDao;
            _serverSettings = serverSettings;
            _logger = logger;
			_notificationService = notificationService;

            // Since CMT will handle the payment on their ends. We do not need to know the actual company of the cab from wich we do the manual pairing.
            _cmtMobileServiceClient = new CmtMobileServiceClient(_serverSettings.GetPaymentSettings().CmtPaymentSettings, null, null, null);
            _cmtTripInfoServiceHelper = new CmtTripInfoServiceHelper(_cmtMobileServiceClient, logger);

            _resources = new Resources.Resources(_serverSettings);
        }

        public ManualRideLinqResponse Get(ManualRideLinqRequest request)
        {
            var order = _orderDao.GetManualRideLinqById(request.OrderId);

            return new ManualRideLinqResponse
            {
                Data = order,
                IsSuccessful = true
            };
        }

        public async Task<ManualRideLinqResponse> Post(ManualRideLinqPairingRequest request)
        {
	        try
	        {
	            var accountId = Session.UserId;
                var account = _accountDao.FindById(accountId);

		        var currentRideLinq = _orderDao.GetCurrentManualRideLinq(request.PairingCode, account.Id);

		        if (currentRideLinq != null)
		        {
			        return new ManualRideLinqResponse
			        {
				        Data = currentRideLinq,
				        IsSuccessful = true,
				        Message = "Ok"
			        };
		        }

                var creditCard = account.DefaultCreditCard.HasValue
                    ? _creditCardDao.FindById(account.DefaultCreditCard.Value)
                    : null;

                if (creditCard == null)
		        {
                    //TODO MKTAXI-3918: handle this
                    throw new HttpException((int)HttpStatusCode.BadRequest, ErrorCode.ManualRideLinq_NoCardOnFile.ToString()/*, _resources.Get("ManualRideLinq_NoCardOnFile", account.Language)*/);
		        }

		        if (creditCard.IsDeactivated)
		        {
                    //TODO MKTAXI-3918: handle this
                    throw new HttpException((int)HttpStatusCode.BadRequest, ErrorCode.ManualRideLinq_CardOnFileDeactivated.ToString()/*, _resources.Get("ManualRideLinq_CreditCardDisabled", account.Language)*/);
		        }

		        // Send pairing request to CMT API
		        var pairingRequest = new ManualRideLinqCoFPairingRequest
		        {
			        AutoTipPercentage = account.DefaultTipPercent ?? _serverSettings.ServerData.DefaultTipPercentage,
			        CustomerId = accountId.ToString(),
			        CustomerName = account.Name,
			        Latitude = request.PickupAddress.Latitude,
			        Longitude = request.PickupAddress.Longitude,
			        PairingCode = request.PairingCode,
			        AutoCompletePayment = true,
                    CardOnFileId = creditCard.Token,
                    LastFour = creditCard.Last4Digits,
                    ZipCode = creditCard.ZipCode,
                    Email = account.Email,
                    CustomerIpAddress = HttpRequest.GetIpAddress(),
                    BillingFullName = creditCard.NameOnCard,
                    SessionId = request.KountSessionId
		        };

		        _logger.LogMessage("Pairing for manual RideLinq with Pairing Code {0}", request.PairingCode);

				var response = await _cmtMobileServiceClient.Post(pairingRequest);

		        _logger.LogMessage("Pairing result: {0}", response.ToJson());

		        var trip = await _cmtTripInfoServiceHelper.WaitForTripInfo(response.PairingToken, response.TimeoutSeconds);

				if (trip.HttpStatusCode == (int)HttpStatusCode.OK)
				{
					var command = new CreateOrderForManualRideLinqPair
					{
						OrderId = Guid.NewGuid(),
						AccountId = accountId,
						UserAgent = HttpRequest.GetUserAgent(),
						ClientVersion = HttpRequest.Headers.GetValues("ClientVersion").FirstOrDefault(),
						PairingCode = request.PairingCode,
						PickupAddress = request.PickupAddress,
						PairingToken = response.PairingToken,
						PairingDate = DateTime.Now,
						ClientLanguageCode = request.ClientLanguageCode,
						Distance = trip.Distance,
						StartTime = trip.StartTime,
						EndTime = trip.EndTime,
						Extra = Math.Round(((double)trip.Extra / 100), 2),
						Fare = Math.Round(((double)trip.Fare / 100), 2),
						Tax = Math.Round(((double)trip.Tax / 100), 2),
						Tip = Math.Round(((double)trip.Tip / 100), 2),
						Toll = trip.TollHistory.Sum(toll => Math.Round(((double)toll.TollAmount / 100), 2)),
						Surcharge = Math.Round(((double)trip.Surcharge / 100), 2),
						Total = Math.Round(((double)trip.Total / 100), 2),
						FareAtAlternateRate = Math.Round(((double)trip.FareAtAlternateRate / 100), 2),
						Medallion = response.Medallion,
						DeviceName = response.DeviceName,
						RateAtTripStart = trip.RateAtTripStart,
						RateAtTripEnd = trip.RateAtTripEnd,
						RateChangeTime = trip.RateChangeTime,
						TripId = trip.TripId,
						DriverId = trip.DriverId,
						LastFour = trip.LastFour,
						AccessFee = Math.Round(((double)trip.AccessFee / 100), 2),
                        OriginatingIpAddress = request.CustomerIpAddress,
                        KountSessionId = request.KountSessionId,
                        CreditCardId = creditCard.CreditCardId,
                    };

					_commandBus.Send(command);

                    _commandBus.Send(new PairForPayment
                    {
                        OrderId = command.OrderId,
                        Medallion = response.Medallion,
                        PairingCode = response.PairingCode,
                        PairingToken = response.PairingToken,
                        DriverId = trip.DriverId.ToString(),
                        TokenOfCardToBeUsedForPayment = creditCard.Token
                    });

					var data = new OrderManualRideLinqDetail
					{
						OrderId = command.OrderId,
						Distance = trip.Distance,
						StartTime = trip.StartTime,
						EndTime = trip.EndTime,
						Extra = command.Extra,
						Fare = command.Fare,
						Tax = command.Tax,
						Tip = command.Tip,
						Toll = command.Toll,
						Surcharge = command.Surcharge,
						Total = command.Total,
						FareAtAlternateRate = command.FareAtAlternateRate,
						Medallion = response.Medallion,
						DeviceName = response.DeviceName,
						RateAtTripStart = command.RateAtTripStart,
						RateAtTripEnd = command.RateAtTripEnd,
						RateChangeTime = trip.RateChangeTime,
						AccountId = accountId,
						PairingDate = command.PairingDate,
						PairingCode = pairingRequest.PairingCode,
						PairingToken = trip.PairingToken,
						DriverId = trip.DriverId,
						LastFour = command.LastFour,
						AccessFee = command.AccessFee
					};

					return new ManualRideLinqResponse
					{
						Data = data,
						IsSuccessful = true,
						Message = "Ok",
						TripInfoHttpStatusCode = trip.HttpStatusCode,
						ErrorCode = trip.ErrorCode.ToString()
					};
				}
				else
				{
					if (trip.HttpStatusCode == (int)HttpStatusCode.BadRequest)
					{
						switch (trip.ErrorCode)
						{
							case CmtErrorCodes.CreditCardDeclinedOnPreauthorization:
								_notificationService.SendCmtPaymentFailedPush(accountId, _resources.Get("CreditCardDeclinedOnPreauthorizationErrorText", request.ClientLanguageCode));
								break;
							case CmtErrorCodes.UnablePreauthorizeCreditCard:
                                _notificationService.SendCmtPaymentFailedPush(accountId, _resources.Get("CreditCardUnableToPreathorizeErrorText", request.ClientLanguageCode));
								break;
							default:
								_notificationService.SendCmtPaymentFailedPush(accountId, _resources.Get("TripUnableToPairErrorText", request.ClientLanguageCode));
								break;
						}
					}

					return new ManualRideLinqResponse
					{
						IsSuccessful = false,
						TripInfoHttpStatusCode = trip.HttpStatusCode,
						ErrorCode = trip.ErrorCode != null ? trip.ErrorCode.ToString() : null
					};
				}
	        }
	        catch (WebServiceException ex)
	        {
		        _logger.LogMessage(
			        string.Format("A WebServiceException occured while trying to manually pair with CMT with pairing code: {0}",
				        request.PairingCode));
		        _logger.LogError(ex);

		        ErrorResponse errorResponse = null;

		        if (ex.ResponseBody != null)
		        {
			        _logger.LogMessage("Error Response: {0}", ex.ResponseBody);

			        errorResponse = ex.ResponseBody.FromJson<ErrorResponse>();
		        }

		        return new ManualRideLinqResponse
		        {
			        IsSuccessful = false,
			        Message = errorResponse != null ? errorResponse.Message : ex.ErrorMessage,
					ErrorCode = errorResponse != null ? errorResponse.ResponseCode.ToString() : ex.ErrorCode
		        };
	        }
            catch (Exception ex)
            {
                _logger.LogMessage(string.Format("An error occured while trying to manually pair with CMT with pairing code: {0}", request.PairingCode));
                _logger.LogError(ex);

                return new ManualRideLinqResponse
                {
                    IsSuccessful = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ManualRideLinqResponse> Put(ManualRideLinqUpdateAutoTipRequest request)
        {
            var ridelinqOrderDetail = _orderDao.GetManualRideLinqById(request.OrderId);
            if (ridelinqOrderDetail == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Order not found");
            }

            try
            {
                var accountId = Session.UserId;
                var account = _accountDao.FindById(accountId);
                var orderDetail = _orderDao.FindById(request.OrderId);

                var tipUpdateRequest = new CMTPayment.Pair.ManualRideLinqPairingRequest
                {
                    AutoTipPercentage = request.AutoTipPercentage,
                    CustomerId = accountId.ToString(),
                    CustomerName = account.Name,
                    Latitude = orderDetail.PickupAddress.Latitude,
                    Longitude = orderDetail.PickupAddress.Longitude,
                    AutoCompletePayment = true
                };

                var response = await _cmtMobileServiceClient.Put(string.Format("init/pairing/{0}", ridelinqOrderDetail.PairingToken), tipUpdateRequest);

                // Wait for trip to be updated
                await _cmtTripInfoServiceHelper.WaitForTipUpdated(ridelinqOrderDetail.PairingToken, request.AutoTipPercentage, response.TimeoutSeconds);

                return new ManualRideLinqResponse
                {
                    IsSuccessful = true
                };
            }
            catch (WebServiceException ex)
            {
                _logger.LogMessage(string.Format("A WebServiceException occured while trying to update CMT pairing for OrderId: {0} with pairing token: {1}", request.OrderId, ridelinqOrderDetail.PairingToken));
                _logger.LogError(ex);

                ErrorResponse errorResponse = null;

                if (ex.ResponseBody != null)
                {
                    _logger.LogMessage("Error Response: {0}", ex.ResponseBody);

                    errorResponse = ex.ResponseBody.FromJson<ErrorResponse>();
                }

                return new ManualRideLinqResponse
                {
                    IsSuccessful = false,
                    Message = errorResponse != null ? errorResponse.Message : ex.ErrorMessage,
                    ErrorCode = errorResponse != null ? errorResponse.ResponseCode.ToString() : ex.ErrorCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogMessage(string.Format("An error occured while trying to update CMT pairing for OrderId: {0} with pairing token: {1}", request.OrderId, ridelinqOrderDetail.PairingToken));
                _logger.LogError(ex);

                return new ManualRideLinqResponse
                {
                    IsSuccessful = false,
                    Message = ex.Message
                };
            }
        }

        public async Task Delete(ManualRideLinqRequest request)
        {
            var order = _orderDao.GetManualRideLinqById(request.OrderId);
            if (order == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Order not found");
            }

            try
            {
                var response = await _cmtMobileServiceClient.Delete<CmtUnpairingResponse>(string.Format("init/pairing/{0}", order.PairingToken));

                // Wait for trip to be updated
                await _cmtTripInfoServiceHelper.WaitForRideLinqUnpaired(order.PairingToken, response.TimeoutSeconds);

                _commandBus.Send(new UnpairOrderForManualRideLinq {OrderId = request.OrderId});
            }
            catch (WebServiceException ex)
            {
                _logger.LogMessage(string.Format("A WebServiceException occured while trying to manually unpair with CMT for OrderId: {0} with pairing token: {1}", request.OrderId, order.PairingToken));
                _logger.LogError(ex);

                string errorResponse = null;

                if (ex.ResponseBody != null)
                {
                    errorResponse = ex.ResponseBody;
                    _logger.LogMessage("Error Response: {0}", errorResponse);
                }

                throw new HttpException((int)HttpStatusCode.InternalServerError, errorResponse ?? ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogMessage(string.Format("An error occured while trying to manually unpair with CMT for OrderId: {0} with pairing token: {1}", request.OrderId, order.PairingToken));
                _logger.LogError(ex);

                throw new HttpException((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
