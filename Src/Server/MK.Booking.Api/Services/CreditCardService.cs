#region

using System;
using System.Linq;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using AutoMapper;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class CreditCardService : Service
    {
        private readonly ICommandBus _bus;
        private readonly IOrderDao _orderDao;
        private readonly ICreditCardDao _dao;

        public CreditCardService(ICreditCardDao dao, ICommandBus bus, IOrderDao orderDao)
        {
            _bus = bus;
            _orderDao = orderDao;
            _dao = dao;
        }

        public object Get(CreditCardRequest request)
        {
            var session = this.GetSession();
            return _dao.FindByAccountId(new Guid(session.UserAuthId));
        }

        public object Post(CreditCardRequest request)
        {
            var session = this.GetSession();
            var command = new AddCreditCard {AccountId = new Guid(session.UserAuthId)};
            Mapper.Map(request, command);

            _bus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }

        public object Put(CreditCardRequest request)
        {
            var session = this.GetSession();
            var command = new UpdateCreditCard { AccountId = new Guid(session.UserAuthId) };
            Mapper.Map(request, command);

            _bus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }

        public object Delete(CreditCardRequest request)
        {
            var session = this.GetSession();
            var accountId = new Guid(session.UserAuthId);

            var activeOrder = _orderDao.GetOrdersInProgressByAccountId(accountId);
            if (activeOrder.Any())
            {
                throw new HttpError("Can't delete credit card when an order is in progress");
            }

            var command = new DeleteAccountCreditCards
            {
                AccountId = accountId
            };

            _bus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}