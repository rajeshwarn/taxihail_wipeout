using System;
using System.Net;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Api.Services
{
    public class BookingSettingsService : RestServiceBase<BookingSettingsRequest>
    {
        private readonly ICommandBus _commandBus;

        public BookingSettingsService(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }

        public override object OnPut(BookingSettingsRequest request)
        {
            var command = new UpdateBookingSettings();
            AutoMapper.Mapper.Map(request, command);

        
              
            command.AccountId = new Guid(this.GetSession().UserAuthId);

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        
        }

     
    }
}