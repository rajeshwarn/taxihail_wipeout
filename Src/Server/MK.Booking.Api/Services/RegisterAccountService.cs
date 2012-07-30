using System;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Resources;
using Infrastructure.Messaging;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query;

using RegisterAccount = apcurium.MK.Booking.Api.Contract.Requests.RegisterAccount;

namespace apcurium.MK.Booking.Api.Services
{
    public class RegisterAccountService : RestServiceBase<RegisterAccount>
    {
        private ICommandBus _commandBus;
        private IAccountDao _accountDao;
        private readonly IAccountWebServiceClient _accountWebServiceClient;

        public RegisterAccountService(ICommandBus commandBus, IAccountWebServiceClient accountWebServiceClient, IAccountDao accountDao)
        {
            _commandBus = commandBus;
            _accountDao = accountDao;
            _accountWebServiceClient = accountWebServiceClient;
        }

        public override object OnPost(RegisterAccount request)
        {
            // Ensure user is not signed in
            this.RequestContext.Get<IHttpRequest>().RemoveSession();

            if (_accountDao.FindByEmail(request.Email) != null )
            {
                throw new HttpError(ErrorCode.CreateAccount_AccountAlreadyExist.ToString()); 
            }

            if (!string.IsNullOrEmpty(request.FacebookId))
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
            else if (!string.IsNullOrEmpty(request.TwitterId))
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
                 var command = new Commands.RegisterAccount();
            AutoMapper.Mapper.Map( request,  command  );
            command.Id = Guid.NewGuid();
            command.IbsAccountId = _accountWebServiceClient.CreateAccount(command.AccountId, 
                                                                            command.Email,
                                                                            "",
                                                                            command.Name,                                                                      
                                                                            command.Phone);
            var confirmationToken = Guid.NewGuid();            
            _commandBus.Send(command);
            _commandBus.Send(new SendAccountConfirmationEmail
                                 {
                                     EmailAddress = command.Email,
                                     ConfirmationUrl = new Uri(new Uri(base.RequestContext.Get<IHttpRequest>().AbsoluteUri), string.Format("/api/account/confirm/{0}/{1}", command.Email, confirmationToken))
                                 });
            return new Account { Id = command.AccountId };
            }
        }

    }
}
