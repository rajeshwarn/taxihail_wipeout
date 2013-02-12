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
        private readonly IConfigurationManager _configurationManager;

        public BookingSettingsService(ICommandBus commandBus, IConfigurationManager configurationManager)
        {
            _commandBus = commandBus;
            _configurationManager = configurationManager;
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