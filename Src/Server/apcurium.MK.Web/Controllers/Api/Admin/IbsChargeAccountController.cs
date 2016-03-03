using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    [RoutePrefix("admin/ibschargeaccount")]
    public class IbsChargeAccountController : BaseApiController
    {
        private readonly IbsChargeAccountService _ibsChargeAccountService;

        public IbsChargeAccountController(IIBSServiceProvider ibsServiceProvider)
        {
            _ibsChargeAccountService = new IbsChargeAccountService(ibsServiceProvider)
            {
                Session = GetSession(),
                HttpRequestContext = RequestContext
            };
        }

        [HttpGet, Route("{accountNumber}/{customerNumber}")]
        public IHttpActionResult GetChargeAccount(string accountNumber, string customerNumber)
        {
            var account = _ibsChargeAccountService.Get(new IbsChargeAccountRequest()
            {
                AccountNumber = accountNumber,
                CustomerNumber = customerNumber
            });

            return GenerateActionResult(account);
        }

        [HttpGet, Auth(Role = RoleName.Admin)]
        public IHttpActionResult GetAll()
        {
            var result = _ibsChargeAccountService.Get();

            return GenerateActionResult(result);
        }

        [HttpPost, Auth(Role = RoleName.Admin)]
        public IHttpActionResult ValidateChargeAccount([FromBody]IbsChargeAccountValidationRequest request)
        {
            var result = _ibsChargeAccountService.Post(request);
            
            return GenerateActionResult(result);
        }
    }
}
