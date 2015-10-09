#region

using System;
using System.Linq;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
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
            var command = new AddOrUpdateCreditCard {AccountId = new Guid(session.UserAuthId)};
            Mapper.Map(request, command);

            _bus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }

        public object Post(DefaultCreditCardRequest request)
        {
            var session = this.GetSession();
            var command = new UpdateDefaultCreditCard { AccountId = new Guid(session.UserAuthId) };
            command.CreditCardId = request.CreditCardId;

            _bus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }

        public object Post(UpdateCreditCardLabelRequest request)
        {
            var session = this.GetSession();
            var accountId = new Guid(session.UserAuthId);

            var creditCardDetails = _dao.FindById(request.CreditCardId);
            if (creditCardDetails == null)
            {
                return new HttpError("Cannot find the credit card");
            }

            var command = new UpdateCreditCardLabel
            {
                AccountId = accountId,
                CreditCardId = request.CreditCardId,
                Label = request.Label
            };

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

            var creditCards = _dao.FindByAccountId(accountId);

            var creditCardDetails = creditCards.FirstOrDefault(x => x.CreditCardId == request.CreditCardId);
            if (creditCardDetails == null)
            {
                return new HttpError("Cannot find the credit card");
            }

            var defaultCreditCard = creditCards.FirstOrDefault(x => x.CreditCardId != request.CreditCardId);
            var command = new DeleteAccountCreditCard
            {
                AccountId = accountId,
                CreditCardId = request.CreditCardId,
                NextDefaultCreditCardId = defaultCreditCard != null ? defaultCreditCard.CreditCardId : (Guid?)null,
            };

            _bus.Send(command);

            return defaultCreditCard;
        }

		public object Delete(DeleteCreditCardsWithAccountRequest request)
		{
			if (_dao.FindByAccountId(request.AccountID).Count > 0)
			{
				_bus.Send(new DeleteAccountCreditCards() { AccountId = request.AccountID });

				return new HttpResult(HttpStatusCode.OK);
			}

			return new HttpError("Cannot find the credit card");
		}
    }
}