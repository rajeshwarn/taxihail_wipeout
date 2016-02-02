using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using CMTPayment;
using CMTPayment.Pair;
using CMTServices;
using CustomerPortal.Client;
using CustomerPortal.Contract.Resources;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services
{
    public class ServerStatusService : Service
    {
        private readonly IServerSettings _serverSettings;
        private readonly IIBSServiceProvider _ibsProvider;
        private readonly ILogger _logger;
        private readonly ITaxiHailNetworkServiceClient _networkService;
        private readonly IOrderStatusUpdateDao _statusUpdaterDao;


        public ServerStatusService(
            IServerSettings serverSettings, 
            IIBSServiceProvider ibsProvider, 
            ILogger logger, 
            ITaxiHailNetworkServiceClient networkService,
            IOrderStatusUpdateDao statusUpdaterDao)
        {
            _serverSettings = serverSettings;
            _ibsProvider = ibsProvider;
            _logger = logger;
            _networkService = networkService;
            _statusUpdaterDao = statusUpdaterDao;
        }

        public object Get(ServerStatusRequest request)
        {
            return GetServiceStatus().Result;
        }

        private async Task<ServiceStatus> GetServiceStatus()
        {
            var ibsTest = RunTest(() => Task.Run(() => _ibsProvider.Booking().GetOrdersStatus(new[] { 0 })), "IBS");

            var useGeo = _serverSettings.ServerData.LocalAvailableVehiclesMode == LocalAvailableVehiclesModes.Geo ||
                _serverSettings.ServerData.ExternalAvailableVehiclesMode == ExternalAvailableVehiclesModes.Geo;

            var useHoneyBadger = _serverSettings.ServerData.LocalAvailableVehiclesMode == LocalAvailableVehiclesModes.HoneyBadger ||
                _serverSettings.ServerData.ExternalAvailableVehiclesMode == ExternalAvailableVehiclesModes.HoneyBadger;

            var paymentSettings = _serverSettings.GetPaymentSettings();
            var useMapi = paymentSettings.PaymentMode == PaymentMethod.Cmt ||
                          paymentSettings.PaymentMode == PaymentMethod.RideLinqCmt;


            var geoTest = useGeo
                ? RunTest(() => Task.Run(() => RunGeoTest()), "GEO")
                : Task.FromResult(false); // We do nothing here.
            
            var honeyBadger = useHoneyBadger
                ? RunTest(() => Task.Run(() => RunHoneyBadgerTest()), "HoneyBadger")
                : Task.FromResult(false); // We do nothing here.

            var orderStatusUpdateDetailTest = Task.Run(() => _statusUpdaterDao.GetLastUpdate());

            var sqlTest = RunTest(async () => await orderStatusUpdateDetailTest, "SQL");

            var mapiTest = useMapi 
                ? RunTest(() => Task.Run(() => RunMapiTest()), "MAPI")
                : Task.FromResult(false);

            var customerPortalTest = RunTest(() => Task.Run(() => _networkService.GetCompanyMarketSettings(_serverSettings.ServerData.GeoLoc.DefaultLatitude, _serverSettings.ServerData.GeoLoc.DefaultLongitude)), "Customer Portal");

            await Task.WhenAll(ibsTest, geoTest, honeyBadger, sqlTest, mapiTest, customerPortalTest).ConfigureAwait(false);

            return new ServiceStatus()
            {
                IsIbsAvailable = ibsTest.Result,
                IbsUrl = _serverSettings.ServerData.IBS.WebServicesUrl,
                IsGeoAvailable = useGeo ? geoTest.Result : (bool?) null,
                GeoUrl = useGeo ? _serverSettings.ServerData.CmtGeo.ServiceUrl : null,
                IsHoneyBadgerAvailable = useHoneyBadger ? honeyBadger.Result : (bool?)null,
                HoneyBadgerUrl = _serverSettings.ServerData.HoneyBadger.ServiceUrl,
                IsSqlAvailable = sqlTest.Result,
                IsMapiAvailable = useMapi ? mapiTest.Result : (bool?) null,
                MapiUrl = useMapi ? GetMapiUrl() : null,
                IsCustomerPortalAvailable = customerPortalTest.Result,
                LastOrderUpdateDate = orderStatusUpdateDetailTest.Result.LastUpdateDate.ToString("U"),
                LastOrderUpdateId = orderStatusUpdateDetailTest.Result.Id.ToString(),
                LastOrderUpdateServer = orderStatusUpdateDetailTest.Result.UpdaterUniqueId
            };
        }

        private string GetMapiUrl()
        {
            var settings = _serverSettings.GetPaymentSettings();

            return settings.CmtPaymentSettings.IsSandbox
                ? settings.CmtPaymentSettings.SandboxMobileBaseUrl
                : settings.CmtPaymentSettings.MobileBaseUrl;
        }


        private void RunMapiTest()
        {
            var cmtMobileServiceClient = new CmtMobileServiceClient(_serverSettings.GetPaymentSettings().CmtPaymentSettings, null, null, null);
            var cmtTripInfoHelper = new CmtTripInfoServiceHelper(cmtMobileServiceClient, _logger);

            var tripInfo = cmtTripInfoHelper.GetTripInfo("0");

            if (tripInfo == null ||  tripInfo.HttpStatusCode == (int) HttpStatusCode.BadRequest && tripInfo.ErrorCode.HasValue)
            {
                return;
            }

            throw new Exception("Mapi connection failed with StatusCode {0} and CmtErrorCode {1}".InvariantCultureFormat(tripInfo.HttpStatusCode, tripInfo.ErrorCode));
        }

        private void RunHoneyBadgerTest()
        {
            var honeyBadgerService = new HoneyBadgerServiceClient(_serverSettings, _logger);

            honeyBadgerService.GetAvailableVehicles(null, _serverSettings.ServerData.GeoLoc.DefaultLatitude, _serverSettings.ServerData.GeoLoc.DefaultLongitude);
        }

        private void RunGeoTest()
        {
            var geoService = new CmtGeoServiceClient(_serverSettings, _logger);

            geoService.GetAvailableVehicles(_serverSettings.ServerData.CmtGeo.AvailableVehiclesMarket, _serverSettings.ServerData.GeoLoc.DefaultLatitude, _serverSettings.ServerData.GeoLoc.DefaultLongitude);
        } 

        private async Task<bool> RunTest(Func<Task> testToRun, string targetService)
        {
            try
            {
                var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                await Task.Run(testToRun, ct.Token).ConfigureAwait(false);

                return true;
            }
            catch (OperationCanceledException)
            {
                // We could not reach the host in under 30 seconds.
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogMessage("An error occurred while attempting to inquire {0} service connection.", targetService);
                _logger.LogError(ex);

                return false;
            }
        }

    }
}
