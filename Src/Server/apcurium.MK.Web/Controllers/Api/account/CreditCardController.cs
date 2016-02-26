using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Web.Security;
using AutoMapper;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Account
{
    [RoutePrefix("account")]
    public class CreditCardController : BaseApiController
    {
        private readonly IOrderDao _orderDao;
        private readonly ICreditCardDao _creditCardDao;
        private readonly ICommandBus _commandBus;

        public CreditCardController(IOrderDao orderDao, ICreditCardDao creditCardDao, ICommandBus commandBus)
        {
            _orderDao = orderDao;
            _creditCardDao = creditCardDao;
            _commandBus = commandBus;
        }

        [HttpGet, Auth, Route("creditcards")]
        public IList<CreditCardDetails> GetCreditCards()
        {
            return _creditCardDao.FindByAccountId(GetSession().UserId);
        }

        [HttpGet, Auth, Route("creditcardinfo/{creditCardId}")]
        public CreditCardDetails GetCreditCardInfo(Guid creditCardId)
        {
            return _creditCardDao.FindById(creditCardId);
        }
        [HttpPost, Auth, Route("creditcards")]
        public HttpResponseMessage Post(CreditCardRequest request)
        {
            var session = GetSession();

            request.Label = request.Label ?? CreditCardLabelConstants.Personal.ToString();

            var command = new AddOrUpdateCreditCard { AccountId = session.UserId };
            Mapper.Map(request, command);

            _commandBus.Send(command);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost, Auth, Route("creditcard/updatedefault")]
        public object Post(DefaultCreditCardRequest request)
        {
            var session = GetSession();
            var command = new UpdateDefaultCreditCard
            {
                AccountId = session.UserId,
                CreditCardId = request.CreditCardId
            };

            _commandBus.Send(command);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost, Auth, Route("creditcard/updatelabel")]
        public HttpResponseMessage Post(UpdateCreditCardLabelRequest request)
        {
            var session = GetSession();
            var accountId = session.UserId;

            var creditCardDetails = _creditCardDao.FindById(request.CreditCardId);
            if (creditCardDetails == null)
            {
                throw new HttpException("Cannot find the credit card");
            }

            var command = new UpdateCreditCardLabel
            {
                AccountId = accountId,
                CreditCardId = request.CreditCardId,
                Label = request.Label
            };

            _commandBus.Send(command);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpDelete, Auth, Route("creditcards/{creditCardId}")]
        public CreditCardDetails DeleteCreditCard(Guid creditCardId)
        {
            var session = GetSession();
            var accountId = session.UserId;

            var activeOrder = _orderDao.GetOrdersInProgressByAccountId(accountId);
            if (activeOrder.Any())
            {
                throw new HttpException("Can't delete credit card when an order is in progress");
            }

            var creditCards = _creditCardDao.FindByAccountId(accountId);

            var creditCardDetails = creditCards.FirstOrDefault(x => x.CreditCardId == creditCardId);
            if (creditCardDetails == null)
            {
                throw new HttpException("Cannot find the credit card");
            }

            var defaultCreditCard = creditCards.FirstOrDefault(x => x.CreditCardId != creditCardId);
            var command = new DeleteAccountCreditCard
            {
                AccountId = accountId,
                CreditCardId = creditCardId,
                NextDefaultCreditCardId = defaultCreditCard != null ? defaultCreditCard.CreditCardId : (Guid?)null,
            };

            _commandBus.Send(command);

            return defaultCreditCard;
        }
    }
}
