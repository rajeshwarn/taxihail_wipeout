using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [AuthorizationRequired(ApplyTo.All, RoleName.Admin)]
    [Route("/account/grantadmin", "PUT")]
    public class GrantAdminRightRequest : BaseDTO
    {
        public string AccountEmail { get; set; }
    }

    [Authenticate]
    [AuthorizationRequired(ApplyTo.All, RoleName.SuperAdmin)]
    [Route("/account/grantsuperadmin", "PUT")]
    public class GrantSuperAdminRightRequest : GrantAdminRightRequest
    {
    }
}
