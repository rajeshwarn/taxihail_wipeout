using System.Net;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Services
{
    public class AdminConfirmAccountService : RestServiceBase<AdminConfirmAccountRequest>
    {
        public AdminConfirmAccountService(IAccountDao dao, ICommandBus commandBus)
        {
            Dao = dao;
            _commandBus = commandBus;
        }

        protected IAccountDao Dao;
        private readonly ICommandBus _commandBus;

        public override object OnPut(AdminConfirmAccountRequest request)
        {
            var account = Dao.FindByEmail(request.AccountEmail);
            if (account == null) throw new HttpError(HttpStatusCode.NotFound, "Not Found");

            _commandBus.Send(new ConfirmAccountByAdmin
                {
                    AccountId = account.Id
                });
            return HttpStatusCode.OK;
        }
    }
}