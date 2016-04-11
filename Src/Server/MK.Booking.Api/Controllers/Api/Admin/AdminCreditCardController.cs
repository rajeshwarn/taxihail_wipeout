using System;
using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    public class AdminCreditCardController : BaseApiController
    {
        public CreditCardService CardService { get; private set; }

        public AdminCreditCardController(CreditCardService cardService)
        {
            CardService = cardService;
        }

        [HttpDelete, Auth(Role = RoleName.Support), Route("api/v2/admin/deleteAllCreditCards/{accountId}")]
        public IHttpActionResult Delete(Guid accountId)
        {
            CardService.Delete(new DeleteCreditCardsWithAccountRequest() {AccountID = accountId});

            return Ok();
        }
    }
}
