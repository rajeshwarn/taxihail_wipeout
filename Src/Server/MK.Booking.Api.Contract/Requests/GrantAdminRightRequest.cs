#region

using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/admin/grantadmin", "PUT")]
    public class GrantAdminRightRequest : BaseDto
    {
        public string AccountEmail { get; set; }
    }

    [Route("/admin/grantsuperadmin", "PUT")]
    public class GrantSuperAdminRightRequest : GrantAdminRightRequest
    {
    }

    [Route("/admin/grantsupport", "PUT")]
    public class GrantSupportRightRequest : GrantAdminRightRequest
    {
    }

    [Route("/admin/revokeaccess", "PUT")]
    public class RevokeAccessRequest : GrantAdminRightRequest
    {
    }
    
}