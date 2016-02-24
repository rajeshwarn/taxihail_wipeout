using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Http;
using apcurium.MK.Web.App_Start;
using Microsoft.Practices.Unity;
using IAuthenticationFilter = System.Web.Http.Filters.IAuthenticationFilter;

namespace apcurium.MK.Web.Security
{
    public class AuthAttribute : Attribute, IAuthenticationFilter
    {
        private readonly ICacheClient _cacheClient;
        private readonly IAccountDao _accountDao;

        public AuthAttribute()
        {
            AllowMultiple = true;
            _accountDao = UnityConfig.GetConfiguredContainer().Resolve<IAccountDao>();
            _cacheClient = UnityConfig.GetConfiguredContainer().Resolve<ICacheClient>();


            Roles = new[] {Booking.Security.Roles.None};
        }

        public bool AllowMultiple { get; }
        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var sessionId = context.Request.Headers.Where(request =>
                    request.Key.Equals("Cookie", StringComparison.InvariantCultureIgnoreCase) &&
                    request.Value.Any(val => val == "ss-opt=perm")
                )
                .Select(request => request.Value.FirstOrDefault(p => p.StartsWith("ss-pid")))
                .Select(pid => pid.Split('=').Last())
                .FirstOrDefault();

            if (!sessionId.HasValueTrimmed())
            {
                context.ErrorResult = new AuthenticationFailureResult("Missing credentials", context.Request);

                return Task.FromResult(0);
            }


            var urn = "urn:iauthsession:{0}".InvariantCultureFormat(sessionId);

            var cachedSession = _cacheClient.Get<SessionEntity>(urn);

            if (cachedSession == null)
            {
                context.ErrorResult = new AuthenticationFailureResult("Invalid username or password", context.Request);

                return Task.FromResult(0);
            }

            var account = _accountDao.FindById(cachedSession.UserId);

            if (Roles.Cast<int>().None(r => account.Roles >= r))
            {
                context.ErrorResult = new AuthenticationFailureResult("User not authorized", context.Request);
            }

            return Task.FromResult(0);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            var challenge = new[]
            {
                new AuthenticationHeaderValue("Basic")
            };
            context.Result = new UnauthorizedResult(challenge, context.Request);
            return Task.FromResult(0);
        }

        public Roles[] Roles { get; set; }
    }
}
