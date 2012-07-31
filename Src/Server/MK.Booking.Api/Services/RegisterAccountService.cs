using System;
using System.Web;
using System.Web.Hosting;
using System.Web.Hosting;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Resources;
using Infrastructure.Messaging;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Extensions;

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
                _commandBus.Send(command);
            
                // Determine the root path to the app 
                var httpRequest = RequestContext.Get<IHttpRequest>();
                string root = new Uri(RequestContext.AbsoluteUri).GetLeftPart(UriPartial.Authority);;
                var aspNetRequest = httpRequest.OriginalRequest as HttpRequest;
                if (aspNetRequest != null)
                {
                    // We are in IIS
                    //The ApplicationVirtualPath property always returns "/" as the first character of the returned value.
                    //If the application is located in the root of the Web site, the return value is just "/".
                    if (HostingEnvironment.ApplicationVirtualPath.Length > 1)
                    {
                        root += HostingEnvironment.ApplicationVirtualPath;
                    }
                }
                else
                {
                    // We are probably in a test environment, using HttpListener
                    // We Assume there is no virtual path
                }

                _commandBus.Send(new SendAccountConfirmationEmail
                {
                    EmailAddress = command.Email,
                    ConfirmationUrl = new Uri(root + string.Format("/api/account/confirm/{0}/{1}", command.Email, confirmationToken)),
                });
                return new Account { Id = command.AccountId };
            }
        }
    }
}
