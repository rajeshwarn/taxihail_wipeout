#region

using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [AuthorizationRequired(ApplyTo.All, RoleName.Admin)]
    [Route("/account/grantadmin", "PUT")]
    public class GrantAdminRightRequest : BaseDto
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