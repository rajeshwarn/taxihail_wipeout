#region

using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class GrantAdminRightService : Service
    {
        private readonly ICommandBus _commandBus;
        protected IAccountDao Dao;

        public GrantAdminRightService(IAccountDao dao, ICommandBus commandBus)
        {
            Dao = dao;
            _commandBus = commandBus;
        }

        public object Put(GrantAdminRightRequest request)
        {
            var account = Dao.FindByEmail(request.AccountEmail);
            if (account == null) throw new HttpError(HttpStatusCode.BadRequest, "Bad request");

            _commandBus.Send(new AddRoleToUserAccount
            {
                AccountId = account.Id,
                RoleName = RoleName.Admin
            });
            return new HttpResult(HttpStatusCode.OK, "OK");
        }

        public object Put(GrantSuperAdminRightRequest request)
        {
            var account = Dao.FindByEmail(request.AccountEmail);
            if (account == null) throw new HttpError(HttpStatusCode.BadRequest, "Bad request");

            _commandBus.Send(new AddRoleToUserAccount
            {
                AccountId = account.Id,
                RoleName = RoleName.SuperAdmin
            });
            return new HttpResult(HttpStatusCode.OK, "OK");
        }
    }
}