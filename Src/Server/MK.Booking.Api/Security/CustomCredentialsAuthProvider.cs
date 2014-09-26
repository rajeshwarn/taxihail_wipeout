#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;

#endregion

namespace apcurium.MK.Booking.Api.Security
{
    public class CustomCredentialsAuthProvider : CredentialsAuthProvider
    {
        private readonly ICommandBus _commandBus;
        private readonly IPasswordService _passwordService;

        public CustomCredentialsAuthProvider(ICommandBus commandBus, IAccountDao dao, IPasswordService passwordService)
        {
            _passwordService = passwordService;
            _commandBus = commandBus;
            Dao = dao;
        }

        protected IAccountDao Dao { get; set; }

        public override bool TryAuthenticate(IServiceBase authService, string userName, string password)
        {
            var account = Dao.FindByEmail(userName);

            return (account != null)
                   && account.IsConfirmed
                   && !account.DisabledByAdmin
                   && _passwordService.IsValid(password, account.Id.ToString(), account.Password);
        }

        public override void OnAuthenticated(IServiceBase authService, IAuthSession session, IOAuthTokens tokens,
            Dictionary<string, string> authInfo)
        {
            var account = Dao.FindByEmail(session.UserAuthName);
            session.UserAuthId = account.Id.ToString();
            session.IsAuthenticated = true;

            session.Permissions = account.RoleNames.ToList();

            authService.SaveSession(session, SessionExpiry);
        }

        public override object Authenticate(IServiceBase authService, IAuthSession session, Auth request)
        {
            try
            {
                var authResponse = (AuthResponse) base.Authenticate(authService, session, request);
                return authResponse;
            }
            catch (Exception)
            {
                var account = Dao.FindByEmail(request.UserName);

                if (account == null ||
                    !_passwordService.IsValid(request.Password, account.Id.ToString(), account.Password))
                {
                    throw HttpError.Unauthorized(AuthenticationErrorCode.InvalidLoginMessage);
                }

                if (account.DisabledByAdmin)
                {
                    throw HttpError.Unauthorized(AuthenticationErrorCode.AccountDisabled);
                }

                if (!account.IsConfirmed)
                {
                    var aspnetReq = (HttpRequest) authService.RequestContext.Get<IHttpRequest>().OriginalRequest;
                    var root = new Uri(aspnetReq.Url, VirtualPathUtility.ToAbsolute("~")).ToString();

                    _commandBus.Send(new SendAccountConfirmationEmail
                    {
                        ClientLanguageCode = account.Language,
                        EmailAddress = account.Email,
                        BaseUrl = new Uri(root),
                        ConfirmationUrl =
                            new Uri(root +
                                    string.Format("/api/account/confirm/{0}/{1}", account.Email,
                                        account.ConfirmationToken)),
                    });
                    throw HttpError.Unauthorized(AuthenticationErrorCode.AccountNotActivated);
                }

                throw;
            }
        }
    }
}