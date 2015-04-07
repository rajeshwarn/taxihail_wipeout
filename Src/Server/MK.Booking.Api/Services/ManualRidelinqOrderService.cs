using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
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
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.Html;
using ServiceStack.ServiceInterface;
using CmtManualRideLinqPairingRequest = CMTPayment.Pair.ManualRideLinqPairingRequest;
using ManualRideLinqPairingRequest = apcurium.MK.Booking.Api.Contract.Requests.Payment.ManualRideLinqPairingRequest;
using CmtManualRideLinqUnpairingRequest = CMTPayment.Pair.ManualRideLinqUnpairingRequest;

namespace apcurium.MK.Booking.Api.Services
{
    public class ManualRidelinqOrderService : Service
    {
        private readonly IOrderDao _orderDao;
        private readonly IAccountDao _accountDao;
        private readonly ICommandBus _commandBus;
        private readonly IServerSettings _serverSettings;
        private readonly CmtMobileServiceClient _cmtMobileServiceClient;
        private readonly CmtTripInfoServiceHelper _cmtTripInfoServiceHelper;

        public ManualRidelinqOrderService(ICommandBus commandBus, IOrderDao orderDao, IAccountDao accountDao, IServerSettings serverSettings, ILogger logger)
        {
            _commandBus = commandBus;
            _orderDao = orderDao;
            _accountDao = accountDao;
            _serverSettings = serverSettings;

            _cmtMobileServiceClient = new CmtMobileServiceClient(_serverSettings.GetPaymentSettings().CmtPaymentSettings, null, null);
            _cmtTripInfoServiceHelper = new CmtTripInfoServiceHelper(_cmtMobileServiceClient, logger);
        }

        public object Post(ManualRideLinqPairingRequest request)
        {
            var accountId = new Guid(this.GetSession().UserAuthId);
            var account = _accountDao.FindById(accountId);

            // send pairing request                                
            var pairingRequest = new CmtManualRideLinqPairingRequest
            {
                AutoTipPercentage = account.DefaultTipPercent ?? _serverSettings.ServerData.DefaultTipPercentage,
                CallbackUrl = string.Empty,
                CustomerId = accountId.ToString(),
                CustomerName = account.Name,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                PairingCode = request.PairingCode,
                AutoCompletePayment = true
            };

            var response = _cmtMobileServiceClient.Post(pairingRequest);

            var trip = _cmtTripInfoServiceHelper.WaitForTripInfo(response.PairingToken, response.TimeoutSeconds);

            var command = new CreateOrderForManualRideLinqPair
            {
                OrderId = Guid.NewGuid(),
                AccountId = accountId,
                UserAgent = Request.UserAgent,
                ClientVersion = Request.Headers.Get("ClientVersion"),
                PairingCode = request.PairingCode,
                PairingToken = response.PairingToken,
                PairingDate = DateTime.Now,
                ClientLanguageCode = request.ClientLanguageCode,
                Distance = trip.Distance,
                EndTime = trip.EndTime,
                Extra = Math.Round(((double) trip.Extra/100), 2),
                Fare = Math.Round(((double) trip.Fare/100), 2),
                Tax = Math.Round(((double) trip.Tax/100), 2),
                Tip = Math.Round(((double) trip.Tip/100), 2),
                Toll = trip.TollHistory.Sum(toll => Math.Round(((double) toll.TollAmount/100), 2)),
                Surcharge = Math.Round(((double) trip.Surcharge/100), 2),
                Total = Math.Round(((double) trip.Total/100), 2),
                FareAtAlternateRate = Math.Round(((double) trip.FareAtAlternateRate/100), 2),
                Medallion = trip.Medallion,
                RateAtTripStart =Math.Round(((double) trip.RateAtTripStart/100), 2),
                RateAtTripEnd = Math.Round(((double) trip.RateAtTripEnd/100), 2),
                RateChangeTime = trip.RateChangeTime,
            };

            _commandBus.Send(command);

            return new OrderManualRideLinqDetail
            {
                OrderId = command.OrderId,
                Distance = trip.Distance,
                EndTime = trip.EndTime,
                Extra = command.Extra,
                Fare = command.Fare,
                Tax = command.Tax,
                Tip = command.Tip,
                Toll = command.Toll,
                Surcharge = command.Surcharge,
                Total = command.Total,
                FareAtAlternateRate = command.FareAtAlternateRate,
                Medallion = trip.Medallion,
                RateAtTripStart = command.RateAtTripStart,
                RateAtTripEnd = command.RateAtTripEnd,
                RateChangeTime = trip.RateChangeTime,
                AccountId = accountId,
                PairingDate = command.PairingDate,
                PairingCode = pairingRequest.PairingCode,
                PairingToken = trip.PairingToken,
            };
        }




        public object Get(ManualRideLinqRequest request)
        {
            var order = _orderDao.GetManualRideLinqById(request.OrderId);

            return new OrderManualRideLinqDetail
            {
                AccountId = order.AccountId,
                Distance = order.Distance,
                EndTime = order.EndTime,
                IsCancelled = order.IsCancelled,
                OrderId = order.OrderId,
                PairingCode = order.PairingCode,
                PairingToken = order.PairingToken,
                PairingDate = order.PairingDate,
                Extra = order.Extra,
                Fare = order.Fare,
                Tax = order.Tax,
                Tip = order.Tip,
                Toll = order.Toll,
                Surcharge = order.Surcharge,
                Total = order.Total,
                FareAtAlternateRate = order.FareAtAlternateRate,
                Medallion = order.Medallion,
                RateAtTripStart = order.RateAtTripStart,
                RateAtTripEnd = order.RateAtTripEnd,
                RateChangeTime = order.RateChangeTime,
            };
        }

        public object Delete(ManualRideLinqRequest request)
        {
            var order = _orderDao.GetManualRideLinqById(request.OrderId);

            var response = _cmtMobileServiceClient.Delete(new CmtManualRideLinqUnpairingRequest
            {
                PairingToken = order.PairingToken
            });

            // wait for trip to be updated
            _cmtTripInfoServiceHelper.WaitForRideLinqUnpaired(order.PairingToken, response.TimeoutSeconds);

            _commandBus.Send(new UnpairOrderForManualRideLinq { OrderId = request.OrderId });

            return new BasePaymentResponse
            {
                IsSuccessful = true,
                Message = "Ok"
            };
        }
    }
}
