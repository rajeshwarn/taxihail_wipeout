#region

using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using AutoMapper;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class SaveAddressService : BaseApiService
    {
        private readonly ICommandBus _commandBus;

        public SaveAddressService(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }

        //TODO MKTAXI-3915: Handle this
        //public IValidator<SaveAddress> Validator { get; set; }

        public void Post(SaveAddress request)
        {
            //TODO MKTAXI-3915: Handle this
            //var result = Validator.Validate(request);

            //if (!result.IsValid)
            //{
            //    throw result.ToException();
            //}

            var command = new AddFavoriteAddress();

            Mapper.Map(request, command);
            command.AccountId = Session.UserId;
            command.Address.Id = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id;
            _commandBus.Send(command);
            
        }

        public void Delete(Guid id)
        {
            var command = new RemoveFavoriteAddress
            {
                Id = Guid.NewGuid(),
                AddressId = id,
                AccountId = Session.UserId
            };

            _commandBus.Send(command);
        }

        public void Put(SaveAddress request)
        {
            var command = new UpdateFavoriteAddress();

            Mapper.Map(request, command);
            command.AccountId = Session.UserId;
            command.Address.Id = request.Id;

            _commandBus.Send(command);
        }
    }
}