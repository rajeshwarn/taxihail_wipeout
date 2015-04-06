using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using CMTPayment;
using CMTPayment.Pair;

namespace apcurium.MK.Booking.Services.Impl
{
    public class ManualRideLinqService : IManualRideLinqService
    {
        private readonly IOrderDao _orderDao;
        private readonly IAccountDao _accountDao;
        private readonly IServerSettings _serverSettings;
        private readonly CmtMobileServiceClient _cmtMobileServiceClient;
        private readonly CmtTripInfoServiceClient _cmtTripInfoServiceClient;


        public ManualRideLinqService(ILogger logger, IServerSettings serverSettings, IOrderDao orderDao, IAccountDao accountDao)
        {
            _serverSettings = serverSettings;
            _orderDao = orderDao;
            _accountDao = accountDao;

            var cmtMobileServiceClient = new CmtMobileServiceClient(serverSettings.GetPaymentSettings().CmtPaymentSettings, null, null);
            _cmtTripInfoServiceClient = new CmtTripInfoServiceClient(cmtMobileServiceClient, logger);
        }

        public Trip PairRideLinqTrip(OrderStatusDetail orderStatusDetail)
        {
            var accountDetail = _accountDao.FindById(orderStatusDetail.AccountId);

            var rideLinqDetails = _orderDao.GetManualRideLinqById(orderStatusDetail.OrderId);

            // send pairing request                                
            var pairingRequest = new ManualRidelinqPairingRequest
            {
                AutoTipPercentage = accountDetail.DefaultTipPercent??0,
                CallbackUrl = string.Empty,
                CustomerId = orderStatusDetail.IBSOrderId.ToString(),
                CustomerName = accountDetail.Name,
                Latitude = orderStatusDetail.VehicleLatitude.GetValueOrDefault(),
                Longitude = orderStatusDetail.VehicleLongitude.GetValueOrDefault(),
                PairingCode = rideLinqDetails.PairingCode,
                AutoCompletePayment = true
            };

            var response = _cmtMobileServiceClient.Post(pairingRequest);

            return _cmtTripInfoServiceClient.WaitForTripInfo(response.PairingToken, response.TimeoutSeconds);
        }

        public Trip GetTripInfo(Guid orderId)
        {
            var rideLinqDetails = _orderDao.GetManualRideLinqById(orderId);

            return _cmtTripInfoServiceClient.GetTripInfo(rideLinqDetails.PairingToken);
        }
    }
}
