using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    public class RoleManagementController : BaseApiController
    {
        public GrantAdminRightService AdminRightService { get; private set; }

        public RoleManagementController(GrantAdminRightService adminRightService)
        {
            AdminRightService = adminRightService;
        }

        [HttpPut, Auth(Role = RoleName.Admin), Route("api/admin/grantadmin")]
        public IHttpActionResult GrantAdminRight(GrantAdminRightRequest request)
        {
            AdminRightService.Put(request);

            return Ok();
        }

        [HttpPut, Auth(Role = RoleName.SuperAdmin), Route("api/admin/grantsuperadmin")]
        public IHttpActionResult GrantSuperAdminRight(GrantSuperAdminRightRequest request)
        {
            AdminRightService.Put(request);

            return Ok();
        }

        [HttpPut, Auth(Role = RoleName.Admin), Route("api/admin/grantsupport")]
        public IHttpActionResult GrantSupportRight(GrantSupportRightRequest request)
        {
            AdminRightService.Put(request);

            return Ok();
        }

        [HttpPut, Auth(Role = RoleName.Admin), Route("api/admin/revokeaccess")]
        public IHttpActionResult GrantRevokeAccessRight(RevokeAccessRequest request)
        {
            AdminRightService.Put(request);

            return Ok();
        }

    }
}
