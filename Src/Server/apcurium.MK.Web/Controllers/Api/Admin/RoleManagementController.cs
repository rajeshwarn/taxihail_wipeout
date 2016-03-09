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
    [RoutePrefix("api/v2/account")]
    public class RoleManagementController : BaseApiController
    {
        private readonly GrantAdminRightService _grantAdminRightService;

        public RoleManagementController(ICommandBus commandBus, IAccountDao accountDao)
        {
            _grantAdminRightService = new GrantAdminRightService(accountDao, commandBus);
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            PrepareApiServices(_grantAdminRightService);
        }

        [HttpPut, Auth(Role = RoleName.Admin), Route("grantadmin")]
        public IHttpActionResult GrantAdminRight(GrantAdminRightRequest request)
        {
            _grantAdminRightService.Put(request);

            return Ok();
        }

        [HttpPut, Auth(Role = RoleName.SuperAdmin), Route("grantsuperadmin")]
        public IHttpActionResult GrantSuperAdminRight(GrantSuperAdminRightRequest request)
        {
            _grantAdminRightService.Put(request);

            return Ok();
        }

        [HttpPut, Auth(Role = RoleName.Admin), Route("grantsupport")]
        public IHttpActionResult GrantSupportRight(GrantSupportRightRequest request)
        {
            _grantAdminRightService.Put(request);

            return Ok();
        }

        [HttpPut, Auth(Role = RoleName.Admin), Route("revokeaccess")]
        public IHttpActionResult GrantRevokeAccessRight(RevokeAccessRequest request)
        {
            _grantAdminRightService.Put(request);

            return Ok();
        }

    }
}
