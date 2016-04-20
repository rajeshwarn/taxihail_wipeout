using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Payment;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Settings
{
    [NoCache]
    public class PaymentSettingsController : BaseApiController
    {
        public PaymentSettingsService PaymentSettingsService { get; private set; }
        public PaymentSettingsController(PaymentSettingsService paymentSettingsService)
        {
            PaymentSettingsService = paymentSettingsService;
        }

        [HttpGet, Auth, Route("api/settings/payments")]
        public IHttpActionResult GetPaymentSettings()
        {
            var result = PaymentSettingsService.Get();

            return GenerateActionResult(result);
        }

        [HttpGet, Auth, Route("api/settings/encrypted/payments")]
        public IHttpActionResult GetEncryptedPaymentSettins()
        {
            var result = PaymentSettingsService.GetEncrypted();

            return GenerateActionResult(result, useCameCase: false);
        }

        [HttpGet, Auth(Role = RoleName.SuperAdmin), Route("api/settings/payments/server")]
        public IHttpActionResult GetServerSettings()
        {
            var result = PaymentSettingsService.GetServerSettings();

            return GenerateActionResult(result);
        }

        [HttpPost, Auth(Role = RoleName.SuperAdmin), Route("api/settings/payments/server")]
        public IHttpActionResult UpdateServerPaymentSettings([FromBody]UpdateServerPaymentSettingsRequest request)
        {
            PaymentSettingsService.Post(request);

            return Ok();
        }

        [HttpPost, Auth(Role = RoleName.SuperAdmin), Route("api/settings/payments/server/test/payPal/production")]
        public IHttpActionResult Post([FromBody] TestPayPalProductionSettingsRequest request)
        {
            var result = PaymentSettingsService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Auth(Role = RoleName.SuperAdmin), Route("api/settings/payments/server/test/payPal/sandbox")]
        public IHttpActionResult Post([FromBody] TestPayPalSandboxSettingsRequest request)
        {
            var result = PaymentSettingsService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Auth(Role = RoleName.SuperAdmin), Route("api/settings/payments/server/test/brainTree")]
        public IHttpActionResult Post([FromBody] TestBraintreeSettingsRequest request)
        {
            var result = PaymentSettingsService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Auth(Role = RoleName.SuperAdmin), Route("api/settings/payments/server/test/cmt")]
        public IHttpActionResult Post([FromBody] TestCmtSettingsRequest request)
        {
            var result = PaymentSettingsService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Auth(Role = RoleName.SuperAdmin), Route("api/settings/payments/server/test/moneris")]
        public IHttpActionResult Post([FromBody] TestMonerisSettingsRequest request)
        {
            var result = PaymentSettingsService.Post(request);

            return GenerateActionResult(result);
        }
    }
}
