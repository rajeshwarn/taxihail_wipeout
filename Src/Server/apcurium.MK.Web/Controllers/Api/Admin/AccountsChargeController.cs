using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Extensions;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    [RoutePrefix("api/admin/accountscharge")]
    public class AccountsChargeController : BaseApiController
    {
        private readonly AccountsChargeService _service;

        public AccountsChargeController(IAccountChargeDao dao, ICommandBus commandBus, IIBSServiceProvider ibsServiceProvider)
        {
            _service = new AccountsChargeService(dao, commandBus, ibsServiceProvider)
            {
                Session = GetSession(),
                HttpRequestContext = RequestContext
            };
        }

        [HttpGet, Route("{accountNumber}"), Route("{accountNumber}/{customerNumber}/{hideAnswers}")]
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

        [HttpGet, Route]
        public IHttpActionResult GetAccountCharge()
        {
            var result = _service.Get(new AccountChargeRequest());

            return GenerateActionResult(result);
        }

        [HttpPost, Route]
        public IHttpActionResult CreateAccountCharge([FromBody]AccountChargeRequest request)
        {
            var result = _service.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPut, Route]
        public IHttpActionResult UpdateAccountCharge([FromBody]AccountChargeRequest request)
        {
            var result = _service.Put(request);

            return GenerateActionResult(result);
        }

        [HttpDelete, Route("{accountNumber}")]
        public IHttpActionResult DeleteAccountCharge(string accountNumber)
        {
            _service.Delete(new AccountChargeRequest {AccountNumber = accountNumber});

            return Ok();
        }
    }
}
