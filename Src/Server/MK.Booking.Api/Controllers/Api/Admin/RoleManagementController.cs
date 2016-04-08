using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    public class RoleManagementController : BaseApiController
    {
        public GrantAdminRightService AdminRightService { get; private set; }

        public RoleManagementController(ICommandBus commandBus, IAccountDao accountDao)
        {
            AdminRightService = new GrantAdminRightService(accountDao, commandBus);
        }

        [HttpPut, Auth(Role = RoleName.Admin), Route("api/v2/admin/grantadmin")]
        public IHttpActionResult GrantAdminRight(GrantAdminRightRequest request)
        {
            AdminRightService.Put(request);

            return Ok();
        }

        [HttpPut, Auth(Role = RoleName.SuperAdmin), Route("api/v2/admin/grantsuperadmin")]
        public IHttpActionResult GrantSuperAdminRight(GrantSuperAdminRightRequest request)
        {
            AdminRightService.Put(request);

            return Ok();
        }

        [HttpPut, Auth(Role = RoleName.Admin), Route("api/v2/admin/grantsupport")]
        public IHttpActionResult GrantSupportRight(GrantSupportRightRequest request)
        {
            AdminRightService.Put(request);

            return Ok();
        }

        [HttpPut, Auth(Role = RoleName.Admin), Route("api/v2/admin/revokeaccess")]
        public IHttpActionResult GrantRevokeAccessRight(RevokeAccessRequest request)
        {
            AdminRightService.Put(request);

            return Ok();
        }

    }
}
