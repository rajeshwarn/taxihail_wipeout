using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    public class AccountsChargeController : BaseApiController
    {
        private readonly AccountsChargeService _service;

        public AccountsChargeController(IAccountChargeDao dao, ICommandBus commandBus,
            IIBSServiceProvider ibsServiceProvider)
        {
            _service = new AccountsChargeService(dao, commandBus, ibsServiceProvider);
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            PrepareApiServices(_service);
        }

        [HttpGet, Route("api/v2/admin/accountscharge/{accountNumber}/{customerNumber}/{hideAnswers}")]
        public IHttpActionResult GetAccountCharge(string accountNumber, string customerNumber, bool hideAnswers)
        {
            var result = _service.Get(new AccountChargeRequest
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
            var result = _service.Get(new AccountChargeRequest());

            return GenerateActionResult(result);
        }

        [HttpPost, Route("api/v2/admin/accountscharge")]
        public IHttpActionResult CreateAccountCharge([FromBody]AccountChargeRequest request)
        {
            var result = _service.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPut, Route("api/v2/admin/accountscharge")]
        public IHttpActionResult UpdateAccountCharge([FromBody]AccountChargeRequest request)
        {
            var result = _service.Put(request);

            return GenerateActionResult(result);
        }

        [HttpDelete, Route("api/v2/admin/accountscharge/{accountNumber}")]
        public IHttpActionResult DeleteAccountCharge(string accountNumber)
        {
            _service.Delete(new AccountChargeRequest {AccountNumber = accountNumber});

            return Ok();
        }
    }
}
