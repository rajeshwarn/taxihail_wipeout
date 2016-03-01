using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    [RoutePrefix("account")]
    public class RoleManagementController : BaseApiController
    {
        private readonly ICommandBus _commandBus;
        private IAccountDao _accountDao;

        public RoleManagementController(ICommandBus commandBus, IAccountDao accountDao)
        {
            _commandBus = commandBus;
            _accountDao = accountDao;
        }

        [HttpPut, Auth(Role = RoleName.Admin), Route("grantadmin")]
        public HttpResponseMessage GrantAdminRight(GrantAdminRightRequest request)
        {
            var account = _accountDao.FindByEmail(request.AccountEmail);
            if (account == null)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Bad request");
            }

            _commandBus.Send(new UpdateRoleToUserAccount
            {
                AccountId = account.Id,
                RoleName = RoleName.Admin
            });

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPut, Auth(Role = RoleName.SuperAdmin), Route("grantsuperadmin")]
        public HttpResponseMessage GrantSuperAdminRight(GrantSuperAdminRightRequest request)
        {
            var account = _accountDao.FindByEmail(request.AccountEmail);
            if (account == null)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Bad request");
            }

            _commandBus.Send(new UpdateRoleToUserAccount
            {
                AccountId = account.Id,
                RoleName = RoleName.SuperAdmin
            });

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPut, Auth(Role = RoleName.Admin), Route("grantsupport")]
        public HttpResponseMessage GrantSupportRight(GrantSupportRightRequest request)
        {
            var account = _accountDao.FindByEmail(request.AccountEmail);
            if (account == null)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Bad request");
            }

            _commandBus.Send(new UpdateRoleToUserAccount
            {
                AccountId = account.Id,
                RoleName = RoleName.Support
            });

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPut, Auth(Role = RoleName.Admin), Route("revokeaccess")]
        public HttpResponseMessage GrantRevokeAccessRight(RevokeAccessRequest request)
        {
            var account = _accountDao.FindByEmail(request.AccountEmail);
            if (account == null)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Bad request");
            }

            _commandBus.Send(new UpdateRoleToUserAccount
            {
                AccountId = account.Id,
                RoleName = RoleName.None
            });

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

    }
}
