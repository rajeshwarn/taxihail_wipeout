#region

using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class AdminAccountManagementService : Service
    {
        private readonly ICommandBus _commandBus;
        protected IAccountDao Dao;

        public AdminAccountManagementService(IAccountDao dao, ICommandBus commandBus)
        {
            Dao = dao;
            _commandBus = commandBus;
        }

        public object Put(EnableAccountByAdminRequest request)
        {
            var account = Dao.FindByEmail(request.AccountEmail);
            if (account == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "Not Found");
            }

            _commandBus.Send(new EnableAccountByAdmin
            {
                AccountId = account.Id
            });
            return HttpStatusCode.OK;
        }

        public object Put(DisableAccountByAdminRequest request)
        {
            var account = Dao.FindByEmail(request.AccountEmail);
            if (account == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "Not Found");
            }

            _commandBus.Send(new DisableAccountByAdmin
            {
                AccountId = account.Id
            });
            return HttpStatusCode.OK;
        }

        public object Put(UnlinkAccountByAdminRequest request)
        {
            var account = Dao.FindByEmail(request.AccountEmail);
            if (account == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "Not Found");
            }

            _commandBus.Send(new UnlinkAccountFromIbs
            {
                AccountId = account.Id
            });
            return HttpStatusCode.OK;
        }
    }
}