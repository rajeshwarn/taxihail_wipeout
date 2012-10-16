using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Security;
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
                if(AccountDao.FindById(new Guid(authSession.UserAuthId)).IsAdmin)
                {
                    authSession.Permissions = new List<string> {Permissions.Admin};
                }
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
