using System;
using System.Linq;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Common.Diagnostic;

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

        public int Priority
        {
            get { return 100; }
        }
    }
}
