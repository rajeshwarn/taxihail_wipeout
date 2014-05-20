using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services
{
    public class AccountsChargeService : Service
    {
        private IAccountChargeDao _dao;
        private ICommandBus _commandBus;

        public AccountsChargeService(IAccountChargeDao dao, ICommandBus commandBus)
        {
            _commandBus = commandBus;
            _dao = dao;
        }

        public object Get(AccountChargeRequest request)
        {
            if (!request.Number.HasValue())
            {
                return _dao.GetAll();
            }
            else
            {
                return _dao.FindByAccountNumber(request.Number);
            }
        }
    }
}