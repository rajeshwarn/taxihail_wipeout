using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Http;
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
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;
using MK.Common.DummyServiceStack;

namespace apcurium.MK.Web.Controllers.Api
{
    [RoutePrefix("api/v2/auth/login"), NoCache]
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
        [Route("facebook")]
        public IHttpActionResult LoginFacebook(Auth request)
        {
            var account = _accountDao.FindByFacebookId(request.UserName);

            if (account == null)
            {
                throw BaseApiService.GenerateException(HttpStatusCode.Unauthorized, "Invalid authentication");
            }

            var authResult = InnerLogin(account);

            return GenerateAuthResult(authResult);
        }

        [HttpPost, Auth, Route("~/api/v2/auth/logout")]
        public IHttpActionResult Logout()
        {
            ForgetSession();
            return Ok();
        }

        [HttpPost]
        [Route("twitter")]
        public IHttpActionResult LoginTwitter(Auth request)
        {
            var account = _accountDao.FindByTwitterId(request.UserName);

            if (account == null)
            {
                throw BaseApiService.GenerateException(HttpStatusCode.Unauthorized, "Invalid authentication");
            }

            var authResult = InnerLogin(account);

            return GenerateAuthResult(authResult);
        }

        public IHttpActionResult GenerateAuthResult(AuthResponse content)
        {
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content.ToJson(), Encoding.Default, "application/json"),
            };

            //httpResponseMessage.Headers.Remove("Cookie");

            //httpResponseMessage.Headers.AddCookies(new[]
            //{
            //    new CookieHeaderValue("ss-pid", content.SessionId),
            //    new CookieHeaderValue("ss-opt", "perm"),  
            //});

            return ResponseMessage(httpResponseMessage);
        }

        [HttpPost]
        [Route("password")]
        public IHttpActionResult Login([FromBody]Auth request)
        {
            var account = _accountDao.FindByEmail(request.UserName);

            var isCredentialsValid = (account != null) &&
                account.IsConfirmed &&
                !account.DisabledByAdmin &&
                _passwordService.IsValid(request.Password, account.Id.ToString(), account.Password);

            if (!isCredentialsValid)
            {
                throw BaseApiService.GenerateException(HttpStatusCode.Unauthorized, "Invalid authentication");
            }

            try
            {
                var authResult = InnerLogin(account);

                return GenerateAuthResult(authResult);
            }
            catch (Exception)
            {
                if (!_passwordService.IsValid(request.Password, account.Id.ToString(), account.Password))
                {
                    throw BaseApiService.GenerateException(HttpStatusCode.Unauthorized, AuthenticationErrorCode.InvalidLoginMessage);
                }

                if (account.DisabledByAdmin)
                {
                    throw BaseApiService.GenerateException(HttpStatusCode.Unauthorized, AuthenticationErrorCode.AccountDisabled);
                }

                if (!account.IsConfirmed)
                {
                    if (account.FacebookId != null)
                    {
                        throw BaseApiService.GenerateException(HttpStatusCode.Unauthorized, AuthenticationErrorCode.FacebookEmailAlreadyUsed);
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

                        var confirmationUrl = "/api/v2/accounts/confirm/{0}/{1}".InvariantCultureFormat(account.Email, account.ConfirmationToken);

                        _commandBus.Send(new SendAccountConfirmationEmail
                        {
                            ClientLanguageCode = account.Language,
                            EmailAddress = account.Email,
                            ConfirmationUrl = new Uri(confirmationUrl, UriKind.Relative),
                        });
                    }
                    throw BaseApiService.GenerateException(HttpStatusCode.Unauthorized, AuthenticationErrorCode.AccountNotActivated);
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
                UserName = account.FacebookId ?? account.TwitterId ?? account.Email
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
            // We check if a session was already generated by ASP.NET
            //var aspSession = Request.Headers
            //    .GetCookies()
            //    .SelectMany(cookieStates => cookieStates.Cookies)
            //    .FirstOrDefault(cookie => cookie.Name == "ASP.NET_SessionId");

            //if (aspSession != null)
            //{
            //    return aspSession.Value;
            //}

            var data = new byte[15];
            lock (_lock)
            {
                _randgen.GetBytes(data);
            }
            return Convert.ToBase64String(data);
        }
    }
}