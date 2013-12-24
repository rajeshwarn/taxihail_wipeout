using System;
using System.Web;
using System.Web.Hosting;
using System.Web.Hosting;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Resources;
using Infrastructure.Messaging;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;

using RegisterAccount = apcurium.MK.Booking.Api.Contract.Requests.RegisterAccount;

namespace apcurium.MK.Booking.Api.Services
{
    public class RegisterAccountService : RestServiceBase<RegisterAccount>
    {
        private ICommandBus _commandBus;
        private IAccountDao _accountDao;
        private IConfigurationManager _configManager;
        private readonly IAccountWebServiceClient _accountWebServiceClient;

        public RegisterAccountService(ICommandBus commandBus, IAccountWebServiceClient accountWebServiceClient, IAccountDao accountDao, IConfigurationManager configManager)
        {
            _commandBus = commandBus;
            _accountDao = accountDao;
            _accountWebServiceClient = accountWebServiceClient;
            _configManager = configManager;
        }

        public override object OnPost(RegisterAccount request)
        {
            // Ensure user is not signed in
            this.RequestContext.Get<IHttpRequest>().RemoveSession();

            if (_accountDao.FindByEmail(request.Email) != null )
            {
                throw new HttpError(ErrorCode.CreateAccount_AccountAlreadyExist.ToString());
            }

            if (request.FacebookId.HasValue())
            {
                if (_accountDao.FindByFacebookId(request.FacebookId) != null) throw new HttpError(ErrorCode.CreateAccount_AccountAlreadyExist.ToString()); 
                var command = new Commands.RegisterFacebookAccount();
                AutoMapper.Mapper.Map(request, command);
                command.Id = Guid.NewGuid();
                command.IbsAccountId = _accountWebServiceClient.CreateAccount(command.AccountId,
                                                                              command.Email,
                                                                              "",
                                                                              command.Name,
                                                                              command.Phone);
                _commandBus.Send(command);
                return new Account { Id = command.AccountId };
            }
            else if (request.TwitterId.HasValue())
            {
                if (_accountDao.FindByTwitterId(request.TwitterId) != null) throw new HttpError(ErrorCode.CreateAccount_AccountAlreadyExist.ToString()); 
                var command = new Commands.RegisterTwitterAccount();
                AutoMapper.Mapper.Map(request, command);
                command.Id = Guid.NewGuid();
                command.IbsAccountId = _accountWebServiceClient.CreateAccount(command.AccountId,
                                                                              command.Email,
                                                                              "",
                                                                              command.Name,
                                                                              command.Phone);
                _commandBus.Send(command);
                return new Account { Id = command.AccountId };
            }
            else
            {
                var setting = _configManager.GetSetting("AccountActivationDisabled");
                var accountActivationDisabled = bool.Parse(string.IsNullOrWhiteSpace(setting) ? bool.FalseString : setting);

                var confirmationToken = Guid.NewGuid();
                var command = new Commands.RegisterAccount();

                AutoMapper.Mapper.Map(request, command);
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
                        EmailAddress = command.Email,
                        ConfirmationUrl = new Uri(root + string.Format("/api/account/confirm/{0}/{1}", command.Email, confirmationToken)),
                    });
                }
                
                return new Account { Id = command.AccountId };
            }
        }
    }
}
