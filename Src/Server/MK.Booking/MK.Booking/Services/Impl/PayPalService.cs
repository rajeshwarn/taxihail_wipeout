using System;
using System.Net;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;

namespace apcurium.MK.Booking.Services.Impl
{
    public class PayPalService
    {
        private readonly IServerSettings _serverSettings;
        private readonly ICommandBus _commandBus;
        private readonly IAccountDao _accountDao;

        public PayPalService(IServerSettings serverSettings, ICommandBus commandBus, IAccountDao accountDao)
        {
            _serverSettings = serverSettings;
            _commandBus = commandBus;
            _accountDao = accountDao;
        }

        public object LinkAccount(Guid accountId, string authCode, string metadataId)
        {
            var account = _accountDao.FindById(accountId);
            if (account == null)
            {
                return new HttpError("Account not found.");
            }

            _commandBus.Send(new LinkPayPalAccount
            {
                AccountId = accountId,
                AuthCode = authCode
            });

            return new HttpResult(HttpStatusCode.OK);
        }

        public object UnlinkAccount(Guid accountId)
        {
            var account = _accountDao.FindById(accountId);
            if (account == null)
            {
                return new HttpError("Account not found.");
            }

            _commandBus.Send(new UnlinkPayPalAccount {  AccountId = accountId });

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}
