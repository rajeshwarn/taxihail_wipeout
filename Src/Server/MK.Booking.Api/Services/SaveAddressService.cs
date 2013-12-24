#region

using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using AutoMapper;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Validation;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class SaveAddressService : Service
    {
        private readonly ICommandBus _commandBus;

        public SaveAddressService(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }

        public IValidator<SaveAddress> Validator { get; set; }

        public object Post(SaveAddress request)
        {
            var result = Validator.Validate(request);

            if (!result.IsValid)
            {
                throw result.ToException();
            }

            var command = new AddFavoriteAddress();

            Mapper.Map(request, command);
            command.AccountId = new Guid(this.GetSession().UserAuthId);
            command.Address.Id = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id;
            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }

        public object Delete(SaveAddress request)
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

        public object Put(SaveAddress request)
        {
            var command = new UpdateFavoriteAddress();

            Mapper.Map(request, command);
            command.AccountId = new Guid(this.GetSession().UserAuthId);
            command.Address.Id = request.Id;

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}