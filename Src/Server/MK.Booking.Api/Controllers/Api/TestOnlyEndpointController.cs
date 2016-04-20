using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Services;

namespace apcurium.MK.Web.Controllers.Api
{
    [RoutePrefix("api/accounts/test")]
    public class TestOnlyEndpointController : BaseApiController
    {
        public TestOnlyReqGetTestAccountService TestOnlyReqGetTestAccountService { get; private set; }
        public TestOnlyReqGetTestAdminAccountService TestOnlyReqGetTestAdminAccountService { get; private set; }

        public TestOnlyEndpointController(TestOnlyReqGetTestAccountService testOnlyReqGetTestAccountService, TestOnlyReqGetTestAdminAccountService testOnlyReqGetTestAdminAccountService)
        {
            TestOnlyReqGetTestAccountService = testOnlyReqGetTestAccountService;
            TestOnlyReqGetTestAdminAccountService = testOnlyReqGetTestAdminAccountService;
        }

        [HttpGet, Route("{index}")]
        public async Task<IHttpActionResult> GetTestAccount(string index)
        {
            var result = await TestOnlyReqGetTestAccountService.GetTestAccount(index);

            return GenerateActionResult(result);
        }

        [HttpGet, Route("admin/{index}")]
        public async Task<IHttpActionResult> GetAdminTestAccount(string index)
        {
            var result = await TestOnlyReqGetTestAdminAccountService.GetTestAdmin(index);

            return GenerateActionResult(result);
        }

    }
}
