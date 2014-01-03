#region

using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using AutoMapper;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class PopularAddressService : Service
    {
        private readonly ICommandBus _commandBus;

        public PopularAddressService(IPopularAddressDao dao, ICommandBus commandBus)
        {
            _commandBus = commandBus;
            Dao = dao;
        }

        public IValidator<PopularAddressService> Validator { get; set; }
        protected IPopularAddressDao Dao { get; set; }

        public object Post(PopularAddress request)
        {
            var command = new AddPopularAddress();

            Mapper.Map(request, command);
            command.Address.Id = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id;

            _commandBus.Send(command);

            return new {command.Address.Id};
        }

        public object Delete(PopularAddress request)
        {
            var command = new RemovePopularAddress
            {
                Id = Guid.NewGuid(),
                AddressId = request.Id
            };

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }

        public object Put(PopularAddress request)
        {
            var command = new UpdatePopularAddress();
            Mapper.Map(request, command);
            command.Address.Id = request.Id;

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}