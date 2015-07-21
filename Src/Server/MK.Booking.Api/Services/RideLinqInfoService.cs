using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Api.Services
{
    public class RideLinqInfoService : Service
    {
        private IOrderDao _orderDao;
        private ILogger _logger;
        private IServerSettings _serverSettings;

        public RideLinqInfoService(IOrderDao orderDao, ILogger logger, IServerSettings serverSettings)
        {
            _orderDao = orderDao;
            _logger = logger;
            _serverSettings = serverSettings;
        }

        public object Get(RidelinqInfoRequest request)
        {
            if (!_serverSettings.ServerData.UsePairingCodeWhenUsingRideLinqCmtPayment)
            {
                return new HttpResult(HttpStatusCode.BadRequest, "Endpoint only compatible with eHail system.");
            }

            var pairingInfo = _orderDao.FindOrderPairingById(request.OrderId);
            if (pairingInfo == null || !pairingInfo.PairingToken.HasValue())
            {
                return new HttpResult(HttpStatusCode.BadRequest, "Endpoint can only be called with an order doen with eHail");
            }

            var tripInfo = GetTripHelper().GetTripInfo(pairingInfo.PairingToken);

            return new RideLinqInfoResponse
            {
                Distance = tripInfo.Distance,
                DriverId = tripInfo.DriverId,
                Extra = tripInfo.Extra * .01,
                Fare = tripInfo.Fare * .01,
                Medallion = tripInfo.Medallion,
                OrderId = request.OrderId,
                Surcharge = tripInfo.Surcharge * .01,
                Tax = tripInfo.Tax * .01,
                Tip = tripInfo.Tip * .01,
                Toll = tripInfo.TollHistory.SelectOrDefault(history => history.Sum(toll => toll.TollAmount * .01)),
                Total = tripInfo.Total * .01
            };
        }


        private CmtTripInfoServiceHelper GetTripHelper()
        {
            var cmtMobileServiceClient = new CmtMobileServiceClient(_serverSettings.GetPaymentSettings().CmtPaymentSettings, null, null);
            return new CmtTripInfoServiceHelper(cmtMobileServiceClient, _logger);
        }

    }
}