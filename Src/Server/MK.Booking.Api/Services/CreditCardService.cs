#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using AutoMapper;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class CreditCardService : BaseApiService
    {
        private readonly IOrderDao _orderDao;
        private readonly IServerSettings _serverSettings;
        private readonly ICreditCardDao _dao;
        private readonly ICommandBus _commandBus;

        public CreditCardService(ICreditCardDao dao, ICommandBus commandBus, IOrderDao orderDao, IServerSettings serverSettings)
        {
            _orderDao = orderDao;
            _serverSettings = serverSettings;
            _dao = dao;
            _commandBus = commandBus;
        }

        public IList<ReadModel.CreditCardDetails> Get()
        {
            return _dao.FindByAccountId(Session.UserId);
        }

		public ReadModel.CreditCardDetails Get(CreditCardInfoRequest request)
		{
			return _dao.FindById(request.CreditCardId);
		}

        public void Post(CreditCardRequest request)
        {
            request.Label = request.Label ?? CreditCardLabelConstants.Personal.ToString();

            var command = new AddOrUpdateCreditCard {AccountId = Session.UserId};
            Mapper.Map(request, command);

            _commandBus.Send(command);
        }

        public void Post(DefaultCreditCardRequest request)
        {
            var command = new UpdateDefaultCreditCard
            {
                AccountId = Session.UserId,
                CreditCardId = request.CreditCardId
            };

            _commandBus.Send(command);
        }

        public void Post(UpdateCreditCardLabelRequest request)
        {
            var creditCardDetails = _dao.FindById(request.CreditCardId);
            if (creditCardDetails == null)
            {
                throw new HttpException("Cannot find the credit card");
            }

            var command = new UpdateCreditCardLabel
            {
                AccountId = Session.UserId,
                CreditCardId = request.CreditCardId,
                Label = request.Label
            };

            _commandBus.Send(command);

            
        }

        public ReadModel.CreditCardDetails Delete(CreditCardRequest request)
        {
            var activeOrder = _orderDao.GetOrdersInProgressByAccountId(Session.UserId);
            if (activeOrder.Any())
            {
                throw new HttpException("Can't delete credit card when an order is in progress");
            }

            var creditCards = _dao.FindByAccountId(Session.UserId);

            var creditCardDetails = creditCards.FirstOrDefault(x => x.CreditCardId == request.CreditCardId);
            if (creditCardDetails == null)
            {
                throw new HttpException("Cannot find the credit card");
            }

            var defaultCreditCard = creditCards.FirstOrDefault(x => x.CreditCardId != request.CreditCardId);
            var command = new DeleteAccountCreditCard
            {
                AccountId = Session.UserId,
                CreditCardId = request.CreditCardId,
                NextDefaultCreditCardId = defaultCreditCard != null ? defaultCreditCard.CreditCardId : (Guid?)null,
            };

            _commandBus.Send(command);

            return defaultCreditCard;
        }

		public void Delete(DeleteCreditCardsWithAccountRequest request)
		{
			if (_dao.FindByAccountId(request.AccountID).Any())
			{
			    var paymentSettings = _serverSettings.GetPaymentSettings();

                var forceUserDisconnect = paymentSettings.CreditCardIsMandatory
                    && paymentSettings.IsPaymentOutOfAppDisabled != OutOfAppPaymentDisabled.None;

                _commandBus.Send(new DeleteCreditCardsFromAccounts
				{
				    AccountIds = new[] { request.AccountID },
                    ForceUserDisconnect = forceUserDisconnect
				});
			}

			throw new HttpException("Cannot find the credit card");
		}
    }
}