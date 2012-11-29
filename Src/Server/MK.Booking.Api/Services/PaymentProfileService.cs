using System;
using System.Net;
using Infrastructure.Messaging;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Services
{
    public class PaymentProfileService : RestServiceBase<UpdatePaymentProfileRequest> 
    {
        private readonly ICommandBus _bus;

        public PaymentProfileService(ICommandBus bus)
        {
            _bus = bus;
        }

        public override object OnPost(UpdatePaymentProfileRequest request)
        {
            var session = this.GetSession();
            var command = new UpdatePaymentProfile { AccountId = new Guid(session.UserAuthId) };
            AutoMapper.Mapper.Map(request, command);

            _bus.Send(command);

            return HttpStatusCode.Accepted;
        }
         
    }
}