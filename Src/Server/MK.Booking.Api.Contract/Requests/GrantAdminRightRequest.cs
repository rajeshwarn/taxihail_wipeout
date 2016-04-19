#region


#endregion

using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/admin/grantadmin", "PUT")]
    public class GrantAdminRightRequest : BaseDto
    {
        public string AccountEmail { get; set; }
    }

    [RouteDescription("/admin/grantsuperadmin", "PUT")]
    public class GrantSuperAdminRightRequest : GrantAdminRightRequest
    {
    }

    [RouteDescription("/admin/grantsupport", "PUT")]
    public class GrantSupportRightRequest : GrantAdminRightRequest
    {
    }

    [RouteDescription("/admin/revokeaccess", "PUT")]
    public class RevokeAccessRequest : GrantAdminRightRequest
    {
    }
    
}