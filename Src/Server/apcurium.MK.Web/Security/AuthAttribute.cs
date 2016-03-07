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
        }

        /// <summary>
        /// Lowest level to access endpoint
        /// </summary>
        /// <remarks> 
        /// By default every users have access. 
        /// 
        /// If we choose restrict to support, the admin and superadmin will have access also.
        /// If we choose to restrict to admin, the superadmin will have access also.
        /// If we choose to restrict to superadmin, only the superadmin will have access
        /// </remarks>
        public string Role { get; set; } = RoleName.None;

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

            if (Role != RoleName.None && account.RoleNames.None(name => name == Role))
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
    }
}
