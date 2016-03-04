#region

using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using AutoMapper;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class PopularAddressService : BaseApiService
    {
        private readonly ICommandBus _commandBus;

        public PopularAddressService(IPopularAddressDao dao, ICommandBus commandBus)
        {
            _commandBus = commandBus;
            Dao = dao;
        }

        //TODO MKTAXI-3915: Handle this
        //public IValidator<PopularAddress> Validator { get; set; }
        protected IPopularAddressDao Dao { get; set; }

        public object Post(PopularAddress request)
        {
            var command = new AddPopularAddress();

            Mapper.Map(request, command);
            command.Address.Id = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id;

            _commandBus.Send(command);

            return new {command.Address.Id};
        }

        public void Delete(Guid addressId)
        {
            var command = new RemovePopularAddress
            {
                Id = Guid.NewGuid(),
                AddressId = addressId
            };

            _commandBus.Send(command);
        }

        public void Put(PopularAddress request)
        {
            var command = new UpdatePopularAddress();
            Mapper.Map(request, command);
            command.Address.Id = request.Id;

            _commandBus.Send(command);
        }

        
    }
}