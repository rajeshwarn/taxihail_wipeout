using System;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Http;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Http;
using Infrastructure.Messaging;
using MK.Common.DummyServiceStack;

namespace apcurium.MK.Web.Controllers.Api
{
    [RoutePrefix("api/auth")]
    public class LoginController : BaseApiController
    {
        private readonly object _lock = new object();
        private readonly RandomNumberGenerator _randgen = new RNGCryptoServiceProvider();

        private readonly ICommandBus _commandBus;
        private readonly IPasswordService _passwordService;
        private readonly IServerSettings _serverSettings;
        private readonly IAccountDao _accountDao;
        private readonly ICacheClient _cacheClient;

        public LoginController(IServerSettings serverSettings, IPasswordService passwordService, ICommandBus commandBus, IAccountDao accountDao, ICacheClient cacheClient)
        {
            _serverSettings = serverSettings;
            _passwordService = passwordService;
            _commandBus = commandBus;
            _accountDao = accountDao;
            _cacheClient = cacheClient;
        }

        [HttpPost]
        [Route("credentialsfb")]
        public AuthResponse LoginFacebook(Auth request)
        {
            var account = _accountDao.FindByFacebookId(request.UserName);

            if (account == null)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Invalid authentication");
            }

            return InnerLogin(account);
        }

        [HttpPost]
        [Route("credentialstw")]
        public AuthResponse LoginTwitter(Auth request)
        {
            var account = _accountDao.FindByTwitterId(request.UserName);

            if (account == null)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Invalid authentication");
            }

            return InnerLogin(account);
        }
        
        [HttpPost]
        [Route("credentials")]
        public AuthResponse Login(Auth request)
        {
            var account = _accountDao.FindByEmail(request.UserName);

            var isCredentialsValid = (account != null) &&
                account.IsConfirmed &&
                !account.DisabledByAdmin &&
                _passwordService.IsValid(request.Password, account.Id.ToString(), account.Password);

            if (!isCredentialsValid)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Invalid authentication");
            }

            try
            {
                return InnerLogin(account);
            }
            catch (Exception)
            {
                if (!_passwordService.IsValid(request.Password, account.Id.ToString(), account.Password))
                {
                    throw new HttpException((int)HttpStatusCode.Unauthorized, AuthenticationErrorCode.InvalidLoginMessage);
                }

                if (account.DisabledByAdmin)
                {
                    throw new HttpException((int)HttpStatusCode.Unauthorized, (AuthenticationErrorCode.AccountDisabled));
                }

                if (!account.IsConfirmed)
                {
                    if (account.FacebookId != null)
                    {
                        throw new HttpException((int)HttpStatusCode.Unauthorized, AuthenticationErrorCode.FacebookEmailAlreadyUsed);
                    }

                    if (_serverSettings.ServerData.SMSConfirmationEnabled)
                    {
                        _commandBus.Send(new SendAccountConfirmationSMS
                        {
                            ClientLanguageCode = account.Language,
                            Code = account.ConfirmationToken,
                            CountryCode = account.Settings.Country,
                            PhoneNumber = account.Settings.Phone
                        });
                    }
                    else
                    {

                        var confirmationUrl = "/api/account/confirm/{0}/{1}".InvariantCultureFormat(account.Email, account.ConfirmationToken);

                        _commandBus.Send(new SendAccountConfirmationEmail
                        {
                            ClientLanguageCode = account.Language,
                            EmailAddress = account.Email,
                            ConfirmationUrl = new Uri(confirmationUrl, UriKind.Relative),
                        });
                    }

                    throw new HttpException((int) HttpStatusCode.Unauthorized, AuthenticationErrorCode.AccountNotActivated);
                }

                throw;
            }
        }

        private AuthResponse InnerLogin(AccountDetail account)
        {
            var sessionId = GenerateSessionId();

            var authResponse = new AuthResponse()
            {
                SessionId = sessionId,
                UserName = account.Email
            };

            var urn = "urn:iauthsession:{0}".InvariantCultureFormat(sessionId);

            var session = new SessionEntity
            {
                SessionId = sessionId,
                UserName = account.Email,
                UserId = account.Id
            };

            _cacheClient.Set(urn, session);

            return authResponse;
        }

        private string GenerateSessionId()
        {
            var data = new byte[15];
            lock (_lock)
            {
                _randgen.GetBytes(data);
            }
            return Convert.ToBase64String(data);
        }
    }
}