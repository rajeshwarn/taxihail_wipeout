using System;
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
    public class AddressHistoryService : RestServiceBase<AddressHistoryRequest>
    {
        private readonly IAddressDao _dao;
        private readonly ICommandBus _commandBus;
        private readonly IAccountDao _accountDao;

        public AddressHistoryService(IAddressDao dao, ICommandBus commandBus, IAccountDao accountDao)
        {
            _dao = dao;
            _commandBus = commandBus;
            _accountDao = accountDao;
        }

        public override object OnGet(AddressHistoryRequest request)
        {
            var session = this.GetSession();
            return _dao.FindHistoricByAccountId(new Guid(session.UserAuthId));
        }

        public override object OnDelete(AddressHistoryRequest request)
        {
            var address = _dao.FindById(request.AddressId);

            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            if (account.Id != address.AccountId)
            {
                throw new HttpError(HttpStatusCode.Unauthorized, "Can't remove another account's address");
            }

            _commandBus.Send(new RemoveAddressFromHistory() { AddressId = request.AddressId, AccountId = account.Id});

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}
