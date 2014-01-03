#region

using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using AutoMapper;
using Infrastructure.Messaging;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class DefaultFavoriteAddressService : Service
    {
        private readonly ICommandBus _commandBus;

        public DefaultFavoriteAddressService(IDefaultAddressDao dao, ICommandBus commandBus)
        {
            _commandBus = commandBus;
            Dao = dao;
        }

        public IValidator<DefaultFavoriteAddressService> Validator { get; set; }
        protected IDefaultAddressDao Dao { get; set; }

        public object Get(DefaultFavoriteAddress request)
        {
            return new DefaultFavoriteAddressResponse(Dao.GetAll());
        }

        public object Post(DefaultFavoriteAddress request)
        {
            var command = new AddDefaultFavoriteAddress();

            Mapper.Map(request, command);
            command.Address.Id = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id;

            _commandBus.Send(command);

            return new {command.Address.Id};
        }

        public object Delete(DefaultFavoriteAddress request)
        {
            var command = new RemoveDefaultFavoriteAddress
            {
                Id = Guid.NewGuid(),
                AddressId = request.Id
            };

            _commandBus.Send(command);

            return string.Empty;
        }

        public object Put(DefaultFavoriteAddress request)
        {
            var command = new UpdateDefaultFavoriteAddress();

            Mapper.Map(request, command);
            command.Address.Id = request.Id;

            _commandBus.Send(command);

            return string.Empty;
        }
    }
}