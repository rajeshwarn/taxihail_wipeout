using System;
using System.Net;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Security;

namespace apcurium.MK.Booking.Api.Services
{
    public class GrantAdminRightService : Service
    {
        public GrantAdminRightService(IAccountDao dao, ICommandBus commandBus)
        {
            Dao = dao;
            _commandBus = commandBus;
        }

        protected IAccountDao Dao;
        private readonly ICommandBus _commandBus;

        public object Put(GrantAdminRightRequest request)
        {
            var account = Dao.FindByEmail(request.AccountEmail);
            if (account == null) throw new HttpError(HttpStatusCode.BadRequest, "Bad request");

            _commandBus.Send(new AddRoleToUserAccount()
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

            _commandBus.Send(new AddRoleToUserAccount()
            {
                AccountId = account.Id,
                RoleName = RoleName.SuperAdmin
            });
            return new HttpResult(HttpStatusCode.OK, "OK");
        }
    }
}
