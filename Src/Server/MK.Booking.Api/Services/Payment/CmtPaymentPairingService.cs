using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using CMTPayment;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class CmtPaymentPairingService : Service
    {
        private readonly IOrderDao _orderDao;
        private readonly IAccountDao _accountDao;
        private readonly ICreditCardDao _creditCardDao;
        private readonly ICommandBus _commandBus;
        private readonly ILogger _logger;
        private readonly IServerSettings _serverSettings;

        // We need to wait until we get a trip info. If Cmt does not give us a timeout, use this default value.
        private const long DefaultTimeoutSeconds = 30; 


        public CmtPaymentPairingService(IOrderDao orderDao, IAccountDao accountDao, ICreditCardDao creditCardDao, ILogger logger, ICommandBus commandBus, IServerSettings serverSettings)
        {
            _orderDao = orderDao;
            _accountDao = accountDao;
            _creditCardDao = creditCardDao;
            _logger = logger;
            _commandBus = commandBus;
            _serverSettings = serverSettings;
        }

        private CmtTripInfoServiceHelper GetTripInfoServiceHelper(string companyKey)
        {
            var cmtMobileServiceClient = new CmtMobileServiceClient(_serverSettings.GetPaymentSettings(companyKey).CmtPaymentSettings, null, null, null);
            return new CmtTripInfoServiceHelper(cmtMobileServiceClient, _logger);
        }

        /// <summary>
        /// This endpoint is used by Cmt to send us the pairing token.
        /// </summary>
        public object Post(CmtPaymentPairingRequest request)
        {
            _logger.LogMessage("Pairing info received for order {0} and PairingToken {1}", request.OrderUuid, request.PairingToken??"Unknown");
            if (Guid.Empty == request.OrderUuid || !request.PairingToken.HasValueTrimmed())
            {
                throw new HttpError(HttpStatusCode.BadRequest, "400", "Missing required parameter");
            }
            
            var orderStatusDetail = _orderDao.FindOrderStatusById(request.OrderUuid);

            if (orderStatusDetail == null)
            {
                throw new HttpError(HttpStatusCode.BadRequest, "401", "Cannot find OrderId");
            }

            var account = _accountDao.FindById(orderStatusDetail.AccountId);

            if (!account.DefaultCreditCard.HasValue)
            {
                throw new HttpError(HttpStatusCode.BadRequest, "402", "User does not have a currently set creditcard");
            }

            var creditCard = _creditCardDao.FindById(account.DefaultCreditCard.Value);

            var tripInfoServiceHelper = GetTripInfoServiceHelper(orderStatusDetail.CompanyKey);

            var tripInfo = tripInfoServiceHelper.WaitForTripInfo(request.PairingToken, request.TimeoutSeconds ?? DefaultTimeoutSeconds);
            
            _commandBus.Send(new PairForPayment
            {
                OrderId = request.OrderUuid,
                Medallion = orderStatusDetail.VehicleNumber,
                DriverId = tripInfo.DriverId.ToString(),
                PairingToken = tripInfo.PairingToken,
                TokenOfCardToBeUsedForPayment = creditCard.Token,
                AutoTipPercentage = tripInfo.AutoTipPercentage
            });
            
            return new CmtPaymentPairingResponse
            {
                CustomerId = account.Id,
                CustomerName =  account.Name,
                CardOnFileId = creditCard.Token,
                TripRequestNumber = orderStatusDetail.IBSStatusId
            };
        }
    }
}
