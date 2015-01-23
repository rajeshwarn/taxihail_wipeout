using System;
using System.Net;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Resources;
using apcurium.MK.Common.Diagnostic;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Services.Impl
{
    public class PayPalService
    {
        private readonly IServerSettings _serverSettings;
        private readonly ICommandBus _commandBus;
        private readonly IAccountDao _accountDao;
        private readonly ILogger _logger;

        public PayPalService(IServerSettings serverSettings, ICommandBus commandBus, IAccountDao accountDao, ILogger logger)
        {
            _serverSettings = serverSettings;
            _commandBus = commandBus;
            _accountDao = accountDao;
            _logger = logger;
        }

        public BasePaymentResponse LinkAccount(Guid accountId, string authCode, string metadataId)
        {
            try
            {
                var account = _accountDao.FindById(accountId);
                if (account == null)
                {
                    throw new Exception("Account not found.");
                }

                _commandBus.Send(new LinkPayPalAccount
                {
                    AccountId = accountId,
                    AuthCode = authCode
                });

                return new BasePaymentResponse
                {
                    IsSuccessful = true,
                    Message = "Success"
                };
            }
            catch (Exception e)
            {
                _logger.LogMessage("PayPal: LinkAccount error");
                _logger.LogError(e);
                return new BasePaymentResponse
                {
                    IsSuccessful = false,
                    Message = e.Message
                };
            }
        }

        public BasePaymentResponse UnlinkAccount(Guid accountId)
        {
            try
            {
                var account = _accountDao.FindById(accountId);
                if (account == null)
                {
                    throw new Exception("Account not found.");
                }

                _commandBus.Send(new UnlinkPayPalAccount { AccountId = accountId });

                return new BasePaymentResponse
                {
                    IsSuccessful = true,
                    Message = "Success"
                };
            }
            catch (Exception e)
            {
                _logger.LogMessage("PayPal: LinkAccount error");
                _logger.LogError(e);
                return new BasePaymentResponse
                {
                    IsSuccessful = false,
                    Message = e.Message
                };
            }
        }
    }
}
