using System.Net;
using Infrastructure.Messaging;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Services
{
    public class GrantAdminRightService : RestServiceBase<GrantAdminRightRequest>
    {
        public GrantAdminRightService(IAccountDao dao, ICommandBus commandBus)
        {
            Dao = dao;
            _commandBus = commandBus;
        }

        protected IAccountDao Dao;
        private readonly ICommandBus _commandBus;

        public override object OnPut(GrantAdminRightRequest request)
        {
            var account = Dao.FindByEmail(request.AccountEmail);
            _commandBus.Send(new GrantAdminRight(){AccountId = account.Id});
            return HttpStatusCode.OK;
        }
    }
}
