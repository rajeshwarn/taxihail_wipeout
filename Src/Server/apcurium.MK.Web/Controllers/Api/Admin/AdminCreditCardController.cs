using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    [RoutePrefix("admin")]
    public class AdminCreditCardController : BaseApiController
    {
        private readonly IServerSettings _serverSettings;
        private readonly ICreditCardDao _creditCardDao;
        private readonly ICommandBus _commandBus;

        public AdminCreditCardController(IServerSettings serverSettings, ICreditCardDao creditCardDao, ICommandBus commandBus)
        {
            _serverSettings = serverSettings;
            _creditCardDao = creditCardDao;
            _commandBus = commandBus;
        }

        [HttpDelete, Auth(Role = RoleName.Support), Route("deleteAllCreditCards/{accountId}")]
        public HttpResponseMessage Delete(Guid accountId)
        {
            if (_creditCardDao.FindByAccountId(accountId).None())
            {
                throw new HttpException("Cannot find the credit card");
            }
            var paymentSettings = _serverSettings.GetPaymentSettings();

            var forceUserDisconnect = paymentSettings.CreditCardIsMandatory
                && paymentSettings.IsPaymentOutOfAppDisabled != OutOfAppPaymentDisabled.None;

            _commandBus.Send(new DeleteCreditCardsFromAccounts
            {
                AccountIds = new[] { accountId },
                ForceUserDisconnect = forceUserDisconnect
            });

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
