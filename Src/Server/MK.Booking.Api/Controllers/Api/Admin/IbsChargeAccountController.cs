using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    public class IbsChargeAccountController : BaseApiController
    {
        public IbsChargeAccountService IBSChargeAccountService { get; private set; }

        public IbsChargeAccountController(IbsChargeAccountService ibsChargeAccountService)
        {
            IBSChargeAccountService = ibsChargeAccountService;
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            PrepareApiServices(IBSChargeAccountService);
        }

        [HttpGet, Route("api/admin/ibschargeaccount/{accountNumber}/{customerNumber}")]
        public IHttpActionResult GetChargeAccount(string accountNumber, string customerNumber)
        {
            var account = IBSChargeAccountService.Get(new IbsChargeAccountRequest()
            {
                AccountNumber = accountNumber,
                CustomerNumber = customerNumber
            });

            return GenerateActionResult(account);
        }

        [HttpGet, Auth(Role = RoleName.Admin), Route("api/admin/ibschargeaccount")]
        public IHttpActionResult GetAll()
        {
            var result = IBSChargeAccountService.Get();

            return GenerateActionResult(result);
        }

        [HttpPost, Auth(Role = RoleName.Admin), Route("api/admin/ibschargeaccount")]
        public IHttpActionResult ValidateChargeAccount([FromBody]IbsChargeAccountValidationRequest request)
        {
            var result = IBSChargeAccountService.Post(request);
            
            return GenerateActionResult(result);
        }
    }
}
