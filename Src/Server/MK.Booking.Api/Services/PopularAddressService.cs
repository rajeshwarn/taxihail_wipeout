using System;
using System.Net;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Services
{
    public class PopularAddressService: RestServiceBase<PopularAddress> 
    {
        public IValidator<PopularAddressService> Validator { get; set; }

        private readonly ICommandBus _commandBus;
        protected IPopularAddressDao Dao { get; set; }
        public PopularAddressService(IPopularAddressDao dao,ICommandBus commandBus)
        {
            _commandBus = commandBus;
            Dao = dao;
        }

        

        public override object OnPost(PopularAddress request)
        {
            var command = new Commands.AddPopularAddress();
            
            AutoMapper.Mapper.Map(request, command);
            command.Address.Id = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id;

            _commandBus.Send(command);

            return new { command.Address.Id };
        }

        public override object OnDelete(PopularAddress request)
        {
            var command = new Commands.RemovePopularAddress
            {
                Id = Guid.NewGuid(),
                AddressId = request.Id
            };

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }

        public override object OnPut(PopularAddress request)
        {
            var command = new Commands.UpdatePopularAddress();
            AutoMapper.Mapper.Map(request, command);
            command.Address.Id = request.Id;

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }

    }
}