#region

using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using AutoMapper;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class BookingSettingsService : Service
    {
        private readonly ICommandBus _commandBus;

        public BookingSettingsService(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }

        public object Put(BookingSettingsRequest request)
        {
            var command = new UpdateBookingSettings();
            Mapper.Map(request, command);


            command.AccountId = new Guid(this.GetSession().UserAuthId);

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}