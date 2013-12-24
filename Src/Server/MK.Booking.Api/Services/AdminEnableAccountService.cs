using System.Net;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Services
{
    public class AdminEnableAccountService : RestServiceBase<EnableAccountByAdminRequest>
    {
        public AdminEnableAccountService(IAccountDao dao, ICommandBus commandBus)
        {
            Dao = dao;
            _commandBus = commandBus;
        }

        protected IAccountDao Dao;
        private readonly ICommandBus _commandBus;

        public override object OnPut(EnableAccountByAdminRequest request)
        {
            var account = Dao.FindByEmail(request.AccountEmail);
            if (account == null) throw new HttpError(HttpStatusCode.NotFound, "Not Found");

            _commandBus.Send(new EnableAccountByAdmin
                {
                    AccountId = account.Id
                });
            return HttpStatusCode.OK;
        }
    }

    public class AdminDisableAccountService : RestServiceBase<DisableAccountByAdminRequest>
    {
        public AdminDisableAccountService(IAccountDao dao, ICommandBus commandBus)
        {
            Dao = dao;
            _commandBus = commandBus;
        }

        protected IAccountDao Dao;
        private readonly ICommandBus _commandBus;

        public override object OnPut(DisableAccountByAdminRequest request)
        {
            var account = Dao.FindByEmail(request.AccountEmail);
            if (account == null) throw new HttpError(HttpStatusCode.NotFound, "Not Found");

            _commandBus.Send(new DisableAccountByAdmin
            {
                AccountId = account.Id
            });
            return HttpStatusCode.OK;
        }
    }
}