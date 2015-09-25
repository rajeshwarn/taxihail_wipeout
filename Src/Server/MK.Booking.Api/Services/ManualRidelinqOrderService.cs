using System;
using System.Linq;
using System.Net;
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
using ServiceStack.Common.Web;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.Text;
using ManualRideLinqPairingRequest = apcurium.MK.Booking.Api.Contract.Requests.Payment.ManualRideLinqPairingRequest;

namespace apcurium.MK.Booking.Api.Services
{
    public class ManualRidelinqOrderService : Service
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

        public ManualRidelinqOrderService(
            ICommandBus commandBus,
            IOrderDao orderDao,
            IAccountDao accountDao,
            ICreditCardDao creditCardDao,
            IServerSettings serverSettings,
            ILogger logger)
        {
            _commandBus = commandBus;
            _orderDao = orderDao;
            _accountDao = accountDao;
            _creditCardDao = creditCardDao;
            _serverSettings = serverSettings;
            _logger = logger;

            // TODO anything to do for manual ridelinq?  when we create an order we have no idea which company we are dispatched to
            _cmtMobileServiceClient = new CmtMobileServiceClient(_serverSettings.GetPaymentSettings().CmtPaymentSettings, null, null);
            _cmtTripInfoServiceHelper = new CmtTripInfoServiceHelper(_cmtMobileServiceClient, logger);

            _resources = new Resources.Resources(_serverSettings);
        }

        public object Get(ManualRideLinqRequest request)
        {
            var order = _orderDao.GetManualRideLinqById(request.OrderId);

            return new ManualRideLinqResponse
            {
                Data = order,
                IsSuccessful = true
            };
        }

        public object Post(ManualRideLinqPairingRequest request)
        {
	        try
	        {
		        var accountId = new Guid(this.GetSession().UserAuthId);
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

		        var creditCard = _creditCardDao.FindByAccountId(account.Id).FirstOrDefault();
		        if (creditCard == null)
		        {
			        throw new HttpError(HttpStatusCode.BadRequest,
				        ErrorCode.ManualRideLinq_NoCardOnFile.ToString(),
				        _resources.Get("ManualRideLinq_NoCardOnFile", account.Language));
		        }

		        if (creditCard.IsDeactivated)
		        {
			        throw new HttpError(HttpStatusCode.BadRequest,
				        ErrorCode.ManualRideLinq_CardOnFileDeactivated.ToString(),
				        _resources.Get("ManualRideLinq_CreditCardDisabled", account.Language));
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
			        CardOnFileId = creditCard.Token
		        };

		        _logger.LogMessage("Pairing for manual RideLinq with Pairing Code {0}", request.PairingCode);

		        var response = _cmtMobileServiceClient.Post(pairingRequest);

		        _logger.LogMessage("Pairing result: {0}", response.ToJson());

		        var trip = _cmtTripInfoServiceHelper.WaitForTripInfo(response.PairingToken, response.TimeoutSeconds);

		        var command = new CreateOrderForManualRideLinqPair
		        {
			        OrderId = Guid.NewGuid(),
			        AccountId = accountId,
			        UserAgent = Request.UserAgent,
			        ClientVersion = Request.Headers.Get("ClientVersion"),
			        PairingCode = request.PairingCode,
			        PickupAddress = request.PickupAddress,
			        PairingToken = response.PairingToken,
			        PairingDate = DateTime.Now,
			        ClientLanguageCode = request.ClientLanguageCode,
			        Distance = trip.Distance,
			        StartTime = trip.StartTime,
			        EndTime = trip.EndTime,
			        Extra = Math.Round(((double) trip.Extra/100), 2),
			        Fare = Math.Round(((double) trip.Fare/100), 2),
			        Tax = Math.Round(((double) trip.Tax/100), 2),
			        Tip = Math.Round(((double) trip.Tip/100), 2),
			        Toll = trip.TollHistory.Sum(toll => Math.Round(((double) toll.TollAmount/100), 2)),
			        Surcharge = Math.Round(((double) trip.Surcharge/100), 2),
			        Total = Math.Round(((double) trip.Total/100), 2),
			        FareAtAlternateRate = Math.Round(((double) trip.FareAtAlternateRate/100), 2),
			        Medallion = response.Medallion,
			        DeviceName = response.DeviceName,
			        RateAtTripStart = trip.RateAtTripStart,
			        RateAtTripEnd = trip.RateAtTripEnd,
			        RateChangeTime = trip.RateChangeTime,
			        TripId = trip.TripId,
			        DriverId = trip.DriverId,
			        LastFour = trip.LastFour,
			        AccessFee = Math.Round(((double) trip.AccessFee/100), 2)
		        };

		        _commandBus.Send(command);

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
			        Message = "Ok"
		        };
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

        public object Put(ManualRideLinqUpdateAutoTipRequest request)
        {
            var ridelinqOrderDetail = _orderDao.GetManualRideLinqById(request.OrderId);
            if (ridelinqOrderDetail == null)
            {
                return new HttpResult(HttpStatusCode.NotFound);
            }

            try
            {
                var accountId = new Guid(this.GetSession().UserAuthId);
                var account = _accountDao.FindById(accountId);
                var orderDetail = _orderDao.FindById(request.OrderId);

                var response =
                    _cmtMobileServiceClient.Put(string.Format("init/pairing/{0}", ridelinqOrderDetail.PairingToken),
                        new CMTPayment.Pair.ManualRideLinqPairingRequest
                        {
                            AutoTipPercentage = request.AutoTipPercentage,
                            CustomerId = accountId.ToString(),
                            CustomerName = account.Name,
                            Latitude = orderDetail.PickupAddress.Latitude,
                            Longitude = orderDetail.PickupAddress.Longitude,
                            AutoCompletePayment = true
                        });

                // Wait for trip to be updated
                _cmtTripInfoServiceHelper.WaitForTipUpdated(ridelinqOrderDetail.PairingToken, request.AutoTipPercentage,
                    response.TimeoutSeconds);

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

        public object Delete(ManualRideLinqRequest request)
        {
            var order = _orderDao.GetManualRideLinqById(request.OrderId);
            if (order == null)
            {
                return new HttpResult(HttpStatusCode.NotFound);
            }

            try
            {
                var response =
                    _cmtMobileServiceClient.Delete<CmtUnpairingResponse>(string.Format("init/pairing/{0}",
                        order.PairingToken));

                // Wait for trip to be updated
                _cmtTripInfoServiceHelper.WaitForRideLinqUnpaired(order.PairingToken, response.TimeoutSeconds);

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

                throw new HttpError(HttpStatusCode.InternalServerError, errorResponse ?? ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogMessage(string.Format("An error occured while trying to manually unpair with CMT for OrderId: {0} with pairing token: {1}", request.OrderId, order.PairingToken));
                _logger.LogError(ex);

                throw new HttpError(HttpStatusCode.InternalServerError, ex.Message);
            }

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}
