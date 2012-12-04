using System;
using System.Net;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Validation;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;

namespace apcurium.MK.Booking.Api.Services
{
    public class SaveAddressService : RestServiceBase<SaveAddress> 
    {
        public IValidator<SaveAddress> Validator { get; set; }

        private readonly ICommandBus _commandBus;
        public SaveAddressService(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }

        public override object OnPost(SaveAddress request)
        {
            var result = Validator.Validate(request);

            if (!result.IsValid)
            {
                throw result.ToException();
            }

            var command = new AddFavoriteAddress();
            
            AutoMapper.Mapper.Map(request, command);
            command.AccountId = new Guid(this.GetSession().UserAuthId);
            command.Address.Id = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id;
            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }

        public override object OnDelete(SaveAddress request)
        {
            var command = new RemoveFavoriteAddress
            {
                Id = Guid.NewGuid(),
                AddressId = request.Id,
                AccountId = new Guid(this.GetSession().UserAuthId)
            };

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }

        public override object OnPut(SaveAddress request)
        {
            var command = new UpdateFavoriteAddress();

            AutoMapper.Mapper.Map(request, command);
            command.AccountId = new Guid(this.GetSession().UserAuthId);
            command.Address.Id = request.Id;

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }

    }
}
