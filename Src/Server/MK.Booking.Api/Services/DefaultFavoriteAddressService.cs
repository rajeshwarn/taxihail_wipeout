using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Validation;
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

            _commandBus.Send(command);

            return new
                       {
                           Id = command.AddressId
                       };
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

            _commandBus.Send(command);

            return string.Empty;
        }

    }
}