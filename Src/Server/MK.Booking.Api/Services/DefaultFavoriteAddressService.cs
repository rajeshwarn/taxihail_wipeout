#region

using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Extensions;
using AutoMapper;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class DefaultFavoriteAddressService : BaseApiService
    {
        private readonly ICommandBus _commandBus;

        public DefaultFavoriteAddressService(IDefaultAddressDao dao, ICommandBus commandBus)
        {
            _commandBus = commandBus;
            Dao = dao;
        }

        protected IDefaultAddressDao Dao { get; set; }

        public DefaultFavoriteAddressResponse Get()
        {
            return new DefaultFavoriteAddressResponse(Dao.GetAll());
        }

        public Guid Post(DefaultFavoriteAddress request)
        {
            var command = new AddDefaultFavoriteAddress();

            Mapper.Map(request, command);
            command.Address.Id = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id;

            _commandBus.Send(command);

            return command.Address.Id;
        }

        public void Delete(DefaultFavoriteAddress request)
        {
            var command = new RemoveDefaultFavoriteAddress
            {
                Id = Guid.NewGuid(),
                AddressId = request.Id
            };

            _commandBus.Send(command);
        }

        public void Put(DefaultFavoriteAddress request)
        {
            var command = new UpdateDefaultFavoriteAddress();

            Mapper.Map(request, command);
            command.Address.Id = request.Id;

            _commandBus.Send(command);
        }
    }
}