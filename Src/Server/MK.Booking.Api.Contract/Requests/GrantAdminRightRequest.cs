#region

using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/account/grantadmin", "PUT")]
    public class GrantAdminRightRequest : BaseDto
    {
        public string AccountEmail { get; set; }
    }

    [Route("/account/grantsuperadmin", "PUT")]
    public class GrantSuperAdminRightRequest : GrantAdminRightRequest
    {
    }

    [Route("/account/grantsupport", "PUT")]
    public class GrantSupportRightRequest : GrantAdminRightRequest
    {
    }

    [Route("/account/revokeaccess", "PUT")]
    public class RevokeAccessRequest : GrantAdminRightRequest
    {
    }
    
}