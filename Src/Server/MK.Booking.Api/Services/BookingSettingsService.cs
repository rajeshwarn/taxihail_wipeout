using System;
using System.Net;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;

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
            if (!request.AccountId.Equals(new Guid(this.GetSession().UserAuthId)))
            {
                throw HttpError.Unauthorized("Unauthorized");
            }

            var command = new UpdateBookingSettings();
            AutoMapper.Mapper.Map(request, command);
            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}