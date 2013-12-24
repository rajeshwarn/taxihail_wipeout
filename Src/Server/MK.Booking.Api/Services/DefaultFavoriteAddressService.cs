using System;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Services
{
    public class DefaultFavoriteAddressService : RestServiceBase<DefaultFavoriteAddress> 
    {
        public IValidator<DefaultFavoriteAddressService> Validator { get; set; }

        private readonly ICommandBus _commandBus;
        protected IDefaultAddressDao Dao { get; set; }
        public DefaultFavoriteAddressService(IDefaultAddressDao dao,ICommandBus commandBus)
        {
            _commandBus = commandBus;
            Dao = dao;
        }

        public override object OnGet(DefaultFavoriteAddress request)
        {
            return new DefaultFavoriteAddressResponse(Dao.GetAll());
        }

        public override object OnPost(DefaultFavoriteAddress request)
        {
            var command = new Commands.AddDefaultFavoriteAddress();
            
            AutoMapper.Mapper.Map(request, command);
            command.Address.Id = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id;

            _commandBus.Send(command);

            return new { command.Address.Id };
        }

        public override object OnDelete(DefaultFavoriteAddress request)
        {
            var command = new Commands.RemoveDefaultFavoriteAddress
            {
                Id = Guid.NewGuid(),
                AddressId = request.Id
            };

            _commandBus.Send(command);

            return string.Empty;
        }

        public override object OnPut(DefaultFavoriteAddress request)
        {
            var command = new Commands.UpdateDefaultFavoriteAddress();

            AutoMapper.Mapper.Map(request, command);
            command.Address.Id = request.Id;

            _commandBus.Send(command);

            return string.Empty;
        }

    }
}