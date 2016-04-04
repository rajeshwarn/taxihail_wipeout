using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Http.Extensions;
using CMTPayment;
using CMTServices;
using CustomerPortal.Client;
using CustomerPortal.Contract.Resources;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services
{
    public class ServerStatusService : BaseApiService
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

        public async Task<ServiceStatus> GetServiceStatus()
        {
            // Setup tests variable
            var useGeo = _serverSettings.ServerData.LocalAvailableVehiclesMode == LocalAvailableVehiclesModes.Geo ||
                _serverSettings.ServerData.ExternalAvailableVehiclesMode == ExternalAvailableVehiclesModes.Geo;

            var useHoneyBadger = _serverSettings.ServerData.LocalAvailableVehiclesMode == LocalAvailableVehiclesModes.HoneyBadger ||
                _serverSettings.ServerData.ExternalAvailableVehiclesMode == ExternalAvailableVehiclesModes.HoneyBadger;

            var paymentSettings = _serverSettings.GetPaymentSettings();

            var useCmtPapi = paymentSettings.PaymentMode == PaymentMethod.Cmt ||
                             paymentSettings.PaymentMode == PaymentMethod.RideLinqCmt;

            // Setup tests
            var ibsTest = RunTest(() => Task.Run(() => _ibsProvider.Booking().GetOrdersStatus(new[] { 0 })), "IBS");

            var geoTest = useGeo
                ? RunTest(() => Task.Run(() => RunGeoTest()), "GEO")
                : Task.FromResult(false); // We do nothing here.
            
            var honeyBadger = useHoneyBadger
                ? RunTest(() => Task.Run(() => RunHoneyBadgerTest()), "HoneyBadger")
                : Task.FromResult(false); // We do nothing here.

            var orderStatusUpdateDetailTest = Task.Run(() => _statusUpdaterDao.GetLastUpdate());

            var sqlTest = RunTest(async () => await orderStatusUpdateDetailTest, "SQL");

            var mapiTest = paymentSettings.PaymentMode == PaymentMethod.RideLinqCmt
                ? RunTest(async () => await RunMapiTest(), "CMT MAPI")
                : Task.FromResult(false);

            var papiTest = useCmtPapi
                ? RunTest(() => Task.Run(() => RunPapiTest(paymentSettings.CmtPaymentSettings)), "CMT PAPI")
                : Task.FromResult(false);

            var customerPortalTest = RunTest(() => Task.Run(() => _networkService.GetCompanyMarketSettings(_serverSettings.ServerData.GeoLoc.DefaultLatitude, _serverSettings.ServerData.GeoLoc.DefaultLongitude)), "Customer Portal");

            // We use ConfigureAwait false here to ensure we are not deadlocking ourselves.
            await Task.WhenAll(ibsTest, geoTest, honeyBadger, sqlTest, mapiTest, papiTest, customerPortalTest).ConfigureAwait(false);

            var orderStatusUpdateDetails = orderStatusUpdateDetailTest.Result;

            var now = DateTime.UtcNow;

            var isUpdaterDeadlocked = orderStatusUpdateDetails.CycleStartDate.HasValue &&
                                      orderStatusUpdateDetails.CycleStartDate + TimeSpan.FromMinutes(10) < now;

            return new ServiceStatus
            {
                IsIbsAvailable = ibsTest.Result,
                IbsUrl = _serverSettings.ServerData.IBS.WebServicesUrl,
                IsGeoAvailable = useGeo ? geoTest.Result : (bool?) null,
                GeoUrl = useGeo ? _serverSettings.ServerData.CmtGeo.ServiceUrl : null,
                IsHoneyBadgerAvailable = useHoneyBadger ? honeyBadger.Result : (bool?)null,
                HoneyBadgerUrl = _serverSettings.ServerData.HoneyBadger.ServiceUrl,
                IsSqlAvailable = sqlTest.Result,
                IsMapiAvailable = paymentSettings.PaymentMode == PaymentMethod.RideLinqCmt ? mapiTest.Result : (bool?) null,
                MapiUrl = paymentSettings.PaymentMode == PaymentMethod.RideLinqCmt ? GetMapiUrl() : null,
                IsPapiAvailable = useCmtPapi ? papiTest.Result : (bool?) null,
                PapiUrl = useCmtPapi ? GetPapiUrl () :null,
                IsCustomerPortalAvailable = customerPortalTest.Result,
                LastOrderUpdateDate = orderStatusUpdateDetails.LastUpdateDate,
                CycleStartDate = orderStatusUpdateDetails.CycleStartDate,
                LastOrderUpdateId = orderStatusUpdateDetails.Id.ToString(),
                LastOrderUpdateServer = orderStatusUpdateDetails.UpdaterUniqueId,
                IsUpdaterDeadlocked = isUpdaterDeadlocked
            };
        }

        private string GetPapiUrl()
        {
            var settings = _serverSettings.GetPaymentSettings();

            return settings.CmtPaymentSettings.IsSandbox
                ? settings.CmtPaymentSettings.SandboxBaseUrl
                : settings.CmtPaymentSettings.BaseUrl;
        }

        private string GetMapiUrl()
        {
            var settings = _serverSettings.GetPaymentSettings();

            return settings.CmtPaymentSettings.IsSandbox
                ? settings.CmtPaymentSettings.SandboxMobileBaseUrl
                : settings.CmtPaymentSettings.MobileBaseUrl;
        }

        private void RunPapiTest(CmtPaymentSettings settings)
        {
            var cc = new TestCreditCards(TestCreditCards.TestCreditCardSetting.Cmt).Visa;
            var result = CmtPaymentClient.TestClient(settings, cc.Number, cc.ExpirationDate, _logger);
            if (result)
            {
                return;
            }

            throw new Exception("Papi connection failed");

        }

        /// <summary>
        /// This no longer tests if the credentials are okay, only if we can communicate with the server
        /// </summary>
        /// <returns></returns>
        private async Task RunMapiTest()
        {
            try
            {
                var cmtSettings = _serverSettings.GetPaymentSettings().CmtPaymentSettings;

                var client = new HttpClient { BaseAddress = new Uri(cmtSettings.IsSandbox
                    ? cmtSettings.SandboxMobileBaseUrl
                    : cmtSettings.MobileBaseUrl)
                };

                var response = await client.GetAsync("hc");
                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    return;
                }

                throw new Exception("Mapi connection failed with StatusCode {0}".InvariantCultureFormat(response != null ? response.StatusCode.ToString() : ""));
            }
            catch (Exception ex)
            {
                throw new Exception("Mapi connection failed with message {0}".InvariantCultureFormat(ex.Message));
            }
        }

        private void RunHoneyBadgerTest()
        {
            var honeyBadgerService = new HoneyBadgerServiceClient(_serverSettings, _logger);

            honeyBadgerService.GetAvailableVehicles(
                null, 
                _serverSettings.ServerData.GeoLoc.DefaultLatitude, 
                _serverSettings.ServerData.GeoLoc.DefaultLongitude,
                throwError: true);
        }

        private void RunGeoTest()
        {
            var geoService = new CmtGeoServiceClient(_serverSettings, _logger);

            geoService.GetAvailableVehicles(
                _serverSettings.ServerData.CmtGeo.AvailableVehiclesMarket,
                _serverSettings.ServerData.GeoLoc.DefaultLatitude,
                _serverSettings.ServerData.GeoLoc.DefaultLongitude,
                throwError:true);
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
