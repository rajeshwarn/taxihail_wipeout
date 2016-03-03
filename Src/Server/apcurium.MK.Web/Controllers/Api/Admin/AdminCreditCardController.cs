using System;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    [RoutePrefix("admin")]
    public class AdminCreditCardController : BaseApiController
    {
        private readonly CreditCardService _creditCardService;

        public AdminCreditCardController(IServerSettings serverSettings, ICreditCardDao creditCardDao, ICommandBus commandBus, IOrderDao orderDao)
        {
            _creditCardService = new CreditCardService(creditCardDao,commandBus, orderDao, serverSettings);
        }

        [HttpDelete, Auth(Role = RoleName.Support), Route("deleteAllCreditCards/{accountId}")]
        public IHttpActionResult Delete(Guid accountId)
        {
            _creditCardService.Delete(new DeleteCreditCardsWithAccountRequest() {AccountID = accountId});

            return Ok();
        }
    }
}
