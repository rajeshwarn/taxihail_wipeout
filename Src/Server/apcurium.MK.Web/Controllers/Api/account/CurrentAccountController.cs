using apcurium.MK.Booking.Api.Contract.Requests;
using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Account
{
    [NoCache]
    public class CurrentAccountController : BaseApiController
    {
        private readonly CurrentAccountService _currentAccountService;

        public CurrentAccountController(IServerSettings serverSettings, ICreditCardDao creditCardDao, IAccountDao accountDao)
        {
            _currentAccountService = new CurrentAccountService(accountDao, creditCardDao, serverSettings);

            PrepareApiServices(_currentAccountService);
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            PrepareApiServices(_currentAccountService);
        }

        [HttpGet, Auth, Route("account")]
        public IHttpActionResult GetCurrentAccount()
        {
            var result = _currentAccountService.Get(new CurrentAccount());

            return GenerateActionResult(result);
        }

        [HttpGet, Route("account/phone/{email}")]
        public IHttpActionResult GetAccountPhone(string email)
        {
            var result = _currentAccountService.Get(new CurrentAccountPhoneRequest() {Email = email});

            return GenerateActionResult(result);
        }
    }
}
