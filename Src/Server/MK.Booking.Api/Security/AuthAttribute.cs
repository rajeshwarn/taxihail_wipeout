﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Http;
using Microsoft.Practices.Unity;
using IAuthenticationFilter = System.Web.Http.Filters.IAuthenticationFilter;
using UnityServiceLocator = apcurium.MK.Common.IoC.UnityServiceLocator;

namespace apcurium.MK.Web.Security
{
    public class AuthAttribute : Attribute, IAuthenticationFilter
    {
        private readonly ICacheClient _cacheClient;
        private readonly IAccountDao _accountDao;

        public AuthAttribute()
        {
            AllowMultiple = true;
            _accountDao = UnityServiceLocator.Instance.Resolve<IAccountDao>();
            _cacheClient = UnityServiceLocator.Instance.Resolve<ICacheClient>();

            Role = RoleName.None;
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
        public string Role { get; set; }

        public bool AllowMultiple { get; }
        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var sessionId = context.Request.GetSessionId();

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
            return Task.FromResult(0);
        }
    }
}
