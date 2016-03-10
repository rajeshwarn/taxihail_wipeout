using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Payment;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Web.Security;
using CustomerPortal.Client;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Settings
{
    [RoutePrefix("api/v2/settings")]
    public class PaymentSettingsController : BaseApiController
    {
        public PaymentSettingsService PaymentSettingsService { get; }
        public PaymentSettingsController(ICommandBus commandBus,
            IConfigurationDao configurationDao,
            ILogger logger,
            IServerSettings serverSettings,
            IPayPalServiceFactory paypalServiceFactory,
            ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient,
            IConfigurationChangeService configurationChangeService)
        {
            PaymentSettingsService = new PaymentSettingsService(commandBus, configurationDao, logger, serverSettings, paypalServiceFactory, taxiHailNetworkServiceClient, configurationChangeService);
        }

        [HttpGet, Auth, Route("payments")]
        public IHttpActionResult GetPaymentSettings()
        {
            var result = PaymentSettingsService.Get();

            return GenerateActionResult(result);
        }

        [HttpGet, Auth, Route("encrypted/payments")]
        public IHttpActionResult GetEncryptedPaymentSettins()
        {
            var result = PaymentSettingsService.GetEncrypted();

            return GenerateActionResult(result);
        }

        [HttpGet, Auth(Role = RoleName.SuperAdmin), Route("payments/server")]
        public IHttpActionResult GetServerSettings()
        {
            var result = PaymentSettingsService.GetServerSettings();

            return GenerateActionResult(result);
        }

        [HttpPost, Auth(Role = RoleName.SuperAdmin), Route("payments/server")]
        public IHttpActionResult UpdateServerPaymentSettings([FromBody]UpdateServerPaymentSettingsRequest request)
        {
            PaymentSettingsService.Post(request);

            return Ok();
        }

        [HttpPost, Auth(Role = RoleName.SuperAdmin), Route("payments/server/test/payPal/production")]
        public IHttpActionResult Post([FromBody] TestPayPalProductionSettingsRequest request)
        {
            var result = PaymentSettingsService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Auth(Role = RoleName.SuperAdmin), Route("payments/server/test/payPal/sandbox")]
        public IHttpActionResult Post([FromBody] TestPayPalSandboxSettingsRequest request)
        {
            var result = PaymentSettingsService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Auth(Role = RoleName.SuperAdmin), Route("payments/server/test/brainTree")]
        public IHttpActionResult Post([FromBody] TestBraintreeSettingsRequest request)
        {
            var result = PaymentSettingsService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Auth(Role = RoleName.SuperAdmin), Route("payments/server/test/cmt")]
        public IHttpActionResult Post([FromBody] TestCmtSettingsRequest request)
        {
            var result = PaymentSettingsService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Auth(Role = RoleName.SuperAdmin), Route("payments/server/test/moneris")]
        public IHttpActionResult Post([FromBody] TestMonerisSettingsRequest request)
        {
            var result = PaymentSettingsService.Post(request);

            return GenerateActionResult(result);
        }
    }
}
