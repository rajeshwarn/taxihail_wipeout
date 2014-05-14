#region

using System;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using AutoMapper;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using RegisterAccount = apcurium.MK.Booking.Api.Contract.Requests.RegisterAccount;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class RegisterAccountService : Service
    {
        private readonly IAccountDao _accountDao;
        private readonly IAccountWebServiceClient _accountWebServiceClient;
        private readonly ICommandBus _commandBus;
        private readonly IConfigurationManager _configManager;

        public RegisterAccountService(ICommandBus commandBus, IAccountWebServiceClient accountWebServiceClient,
            IAccountDao accountDao, IConfigurationManager configManager)
        {
            _commandBus = commandBus;
            _accountDao = accountDao;
            _accountWebServiceClient = accountWebServiceClient;
            _configManager = configManager;
        }

        public object Post(RegisterAccount request)
        {
            // Ensure user is not signed in
            RequestContext.Get<IHttpRequest>().RemoveSession();

            if (_accountDao.FindByEmail(request.Email) != null)
            {
                throw new HttpError(ErrorCode.CreateAccount_AccountAlreadyExist.ToString());
            }

            if (request.FacebookId.HasValue())
            {
                if (_accountDao.FindByFacebookId(request.FacebookId) != null)
                    throw new HttpError(ErrorCode.CreateAccount_AccountAlreadyExist.ToString());
                var command = new RegisterFacebookAccount();
                Mapper.Map(request, command);
                command.Id = Guid.NewGuid();
                command.IbsAccountId = _accountWebServiceClient.CreateAccount(command.AccountId,
                    command.Email,
                    "",
                    command.Name,
                    command.Phone);
                _commandBus.Send(command);
                return new Account {Id = command.AccountId};
            }
            if (request.TwitterId.HasValue())
            {
                if (_accountDao.FindByTwitterId(request.TwitterId) != null)
                    throw new HttpError(ErrorCode.CreateAccount_AccountAlreadyExist.ToString());
                var command = new RegisterTwitterAccount();
                Mapper.Map(request, command);
                command.Id = Guid.NewGuid();
                command.IbsAccountId = _accountWebServiceClient.CreateAccount(command.AccountId,
                    command.Email,
                    "",
                    command.Name,
                    command.Phone);
                _commandBus.Send(command);
                return new Account {Id = command.AccountId};
            }
            else
            {
                var setting = _configManager.GetSetting("AccountActivationDisabled");
                var accountActivationDisabled =
                    bool.Parse(string.IsNullOrWhiteSpace(setting) ? bool.FalseString : setting);

                var confirmationToken = Guid.NewGuid();
                var command = new Commands.RegisterAccount();

                Mapper.Map(request, command);
                command.Id = Guid.NewGuid();
                command.ConfimationToken = confirmationToken.ToString();
                command.IbsAccountId = _accountWebServiceClient.CreateAccount(command.AccountId,
                    command.Email,
                    "",
                    command.Name,
                    command.Phone);

                command.ConfimationToken = confirmationToken.ToString();
                command.AccountActivationDisabled = accountActivationDisabled;
                _commandBus.Send(command);

                // Determine the root path to the app 
                var root = ApplicationPathResolver.GetApplicationPath(RequestContext);

                if (!accountActivationDisabled)
                {
                    _commandBus.Send(new SendAccountConfirmationEmail
                    {
                        ClientLanguageCode = command.Language,
                        EmailAddress = command.Email,
                        ConfirmationUrl =
                            new Uri(root +
                                    string.Format("/api/account/confirm/{0}/{1}", command.Email, confirmationToken)),
                    });
                }

                return new Account {Id = command.AccountId};
            }
        }
    }
}