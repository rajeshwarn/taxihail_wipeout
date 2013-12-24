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
    public class CreditCardService : RestServiceBase<CreditCardRequest> 
    {
        private readonly ICommandBus _bus;
        private readonly ICreditCardDao _dao;

        public CreditCardService(ICreditCardDao dao, ICommandBus bus)
        {
            _bus = bus;
            _dao = dao;
        }

        public override object OnGet(CreditCardRequest request)
        {
            var session = this.GetSession();
            return _dao.FindByAccountId(new Guid(session.UserAuthId));
        }

        public override object OnPost(CreditCardRequest request)
        {
            var session = this.GetSession();
            var command = new AddCreditCard {AccountId = new Guid(session.UserAuthId)};
            AutoMapper.Mapper.Map(request, command);

            _bus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }

        public override object OnDelete(CreditCardRequest request)
        {
            var session = this.GetSession();
            var command = new RemoveCreditCard { AccountId = new Guid(session.UserAuthId), CreditCardId = request.CreditCardId };
           
            _bus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}