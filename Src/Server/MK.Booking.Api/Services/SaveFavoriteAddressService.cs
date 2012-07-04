using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using RegisterAccount = apcurium.MK.Booking.Api.Contract.Requests.RegisterAccount;

namespace apcurium.MK.Booking.Api.Services
{
    public class SaveFavoriteAddressService : RestServiceBase<SaveFavoriteAddress> 
    {
        private readonly ICommandBus _commandBus;
        public SaveFavoriteAddressService(ICommandBus commandBus)
        {
            _commandBus = commandBus;

            AutoMapper.Mapper.CreateMap<SaveFavoriteAddress, Commands.AddFavoriteAddress>();

        }

        public override object OnPost(SaveFavoriteAddress request)
        {
            var command = new Commands.AddFavoriteAddress();
            
            AutoMapper.Mapper.Map(request, command);

            command.Id = Guid.NewGuid();
            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }

        public override object OnDelete(SaveFavoriteAddress request)
        {
            var command = new Commands.RemoveFavoriteAddress
            {
                Id = Guid.NewGuid(),
                AddressId = request.Id,
                AccountId = request.AccountId
            };

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }

    }
}
