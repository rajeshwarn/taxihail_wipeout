using System.Collections.Generic;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;
using AutoMapper;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    [RoutePrefix("admin/ibschargeaccount")]
    public class IbsChargeAccountController : BaseApiController
    {
        private readonly IIBSServiceProvider _ibsServiceProvider;

        public IbsChargeAccountController(IIBSServiceProvider ibsServiceProvider)
        {
            _ibsServiceProvider = ibsServiceProvider;
        }

        [HttpGet, Route("{accountNumber}/{customerNumber}")]
        public IHttpActionResult GetChargeAccount(string accountNumber, string customerNumber)
        {
            var accountFromIbs = _ibsServiceProvider.ChargeAccount().GetIbsAccount(accountNumber, customerNumber);
            var account = new IbsChargeAccount();

            Mapper.Map(accountFromIbs, account);

            return Ok(account);
        }

        [HttpGet, Auth(Role = RoleName.Admin)]
        public IHttpActionResult GetAll()
        {
            var accountsFromIbs = _ibsServiceProvider.ChargeAccount().GetAllAccount();
            var accounts = new List<IbsChargeAccount>();

            Mapper.Map(accountsFromIbs, accounts);

            return Ok(accounts);
        }

        [HttpPost, Auth(Role = RoleName.Admin)]
        public IHttpActionResult ValidateChargeAccount([FromBody]IbsChargeAccountValidationRequest request)
        {
            var ibsAccountValidation = _ibsServiceProvider.ChargeAccount().ValidateIbsChargeAccount(request.Prompts, request.AccountNumber, request.CustomerNumber);
            var accountValidation = new IbsChargeAccountValidation();

            Mapper.Map(ibsAccountValidation, accountValidation);

            return Ok(accountValidation);
        }
    }
}
