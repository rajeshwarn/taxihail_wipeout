#region

using System.Net;
using System.Web;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class GrantAdminRightService : BaseApiService
    {
        private readonly ICommandBus _commandBus;
        protected IAccountDao Dao;

        public GrantAdminRightService(IAccountDao dao, ICommandBus commandBus)
        {
            Dao = dao;
            _commandBus = commandBus;
        }

        public void Put(GrantAdminRightRequest request)
        {
            var account = Dao.FindByEmail(request.AccountEmail);
            if (account == null)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Bad request");
            }

            _commandBus.Send(new UpdateRoleToUserAccount
            {
                AccountId = account.Id,
                RoleName = RoleName.Admin
            });
        }

        public void Put(GrantSuperAdminRightRequest request)
        {
            var account = Dao.FindByEmail(request.AccountEmail);
            if (account == null)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Bad request");
            }


            _commandBus.Send(new UpdateRoleToUserAccount
            {
                AccountId = account.Id,
                RoleName = RoleName.SuperAdmin
            });
        }

        public void Put(GrantSupportRightRequest request)
        {
            var account = Dao.FindByEmail(request.AccountEmail);
            if (account != null)
            {
                _commandBus.Send(new UpdateRoleToUserAccount
                {
                    AccountId = account.Id,
                    RoleName = RoleName.Support
                });
                return;
            }

            throw new HttpException((int)HttpStatusCode.BadRequest, "Account not found");
        }
        public void Put(RevokeAccessRequest request)
        {
            var account = Dao.FindByEmail(request.AccountEmail);
            if (account != null)
            {
                _commandBus.Send(new UpdateRoleToUserAccount
                {
                    AccountId = account.Id,
                    RoleName = RoleName.None
                });
                return;
            }

            throw new HttpException((int)HttpStatusCode.BadRequest, "Account not found");
        }
    }
}