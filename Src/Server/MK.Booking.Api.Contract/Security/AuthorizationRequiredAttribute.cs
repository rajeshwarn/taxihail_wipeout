#region

using System;
using System.Linq;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Diagnostic;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Security
{
    public class AuthorizationRequiredAttribute : RequiredPermissionAttribute
    {
        public AuthorizationRequiredAttribute(ApplyTo applyTo, params string[] permissions)
            : base(applyTo, permissions)
        {
        }

        public IAccountDao AccountDao { get; set; }

        public ILogger Logger { get; set; }

        public int Priority
        {
            get { return 100; }
        }

        public override void Execute(IHttpRequest req, IHttpResponse res, object requestDto)
        {
            var authSession = req.GetSession(true);
            if (authSession != null
                && authSession.UserAuthId != null)
            {
                var account = AccountDao.FindById(new Guid(authSession.UserAuthId));
                authSession.Permissions = account.RoleNames.ToList();
            }
            base.Execute(req, res, requestDto);
        }

        public IHasRequestFilter Copy()
        {
            return this;
        }
    }
}