using System.Linq;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Resources;
using CMTPayment;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
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
            var pairingInfo = _orderDao.FindOrderPairingById(request.OrderId);
            if (pairingInfo == null || !pairingInfo.PairingToken.HasValue())
            {
                return new HttpResult(HttpStatusCode.BadRequest, "Endpoint can only be called with an order done with eHail");
            }

            var tripInfo = GetTripHelper().GetTripInfo(pairingInfo.PairingToken);

            return new RideLinqInfoResponse
            {
                Distance = tripInfo.Distance,
                DriverId = tripInfo.DriverId,
                Extra = tripInfo.Extra / 100,
                Fare = tripInfo.Fare / 100,
                Medallion = tripInfo.Medallion,
                OrderId = request.OrderId,
                Surcharge = tripInfo.Surcharge / 100,
                Tax = tripInfo.Tax / 100,
                Tip = tripInfo.Tip / 100,
                Toll = tripInfo.TollHistory.SelectOrDefault(history => history.Sum(toll => toll.TollAmount / 100)),
                Total = tripInfo.Total / 100,
                PairingCode = pairingInfo.PairingCode
            };
        }

        private CmtTripInfoServiceHelper GetTripHelper()
        {
            var cmtMobileServiceClient = new CmtMobileServiceClient(_serverSettings.GetPaymentSettings().CmtPaymentSettings, null, null);
            return new CmtTripInfoServiceHelper(cmtMobileServiceClient, _logger);
        }
    }
}