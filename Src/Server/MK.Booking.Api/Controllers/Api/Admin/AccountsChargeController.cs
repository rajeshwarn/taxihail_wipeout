using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    public class AccountsChargeController : BaseApiController
    {
        public AccountsChargeService ChargeService { get; private set; }

        public AccountsChargeController(AccountsChargeService chargeService)
        {
            ChargeService = chargeService;
        }

        [HttpGet, Route("api/v2/admin/accountscharge/{accountNumber}/{customerNumber}/{hideAnswers}")]
        public IHttpActionResult GetAccountCharge(string accountNumber, string customerNumber, bool hideAnswers)
        {
            var result = ChargeService.Get(new AccountChargeRequest
            {
                AccountNumber = accountNumber,
                CustomerNumber = customerNumber,
                HideAnswers = hideAnswers
            });

            return GenerateActionResult(result);
        }

        [HttpGet, Route("api/v2/admin/accountscharge/{accountNumber}")]
        public IHttpActionResult GetAccountCharge(string accountNumber)
        {
            return GetAccountCharge(accountNumber, null, false);
        }

        [HttpGet, Route("api/v2/admin/accountscharge")]
        public IHttpActionResult GetAccountCharge()
        {
            var result = ChargeService.Get(new AccountChargeRequest());

            return GenerateActionResult(result);
        }

        [HttpPost, Route("api/v2/admin/accountscharge")]
        public IHttpActionResult CreateAccountCharge([FromBody]AccountChargeRequest request)
        {
            var result = ChargeService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPut, Route("api/v2/admin/accountscharge")]
        public IHttpActionResult UpdateAccountCharge([FromBody]AccountChargeRequest request)
        {
            var result = ChargeService.Put(request);

            return GenerateActionResult(result);
        }

        [HttpDelete, Route("api/v2/admin/accountscharge/{accountNumber}")]
        public IHttpActionResult DeleteAccountCharge(string accountNumber)
        {
            ChargeService.Delete(new AccountChargeRequest {AccountNumber = accountNumber});

            return Ok();
        }
    }
}
