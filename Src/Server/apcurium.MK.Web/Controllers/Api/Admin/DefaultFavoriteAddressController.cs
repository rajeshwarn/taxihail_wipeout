using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;
using AutoMapper;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    [RoutePrefix("admin"), Auth(Role = RoleName.Support)]
    public class DefaultFavoriteAddressController : BaseApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IDefaultAddressDao _defaultAddressDao;

        public DefaultFavoriteAddressController(ICommandBus commandBus, IDefaultAddressDao defaultAddressDao)
        {
            _commandBus = commandBus;
            _defaultAddressDao = defaultAddressDao;
        }

        [HttpGet, Route("addresses"), NoCache]
        public DefaultFavoriteAddressResponse GetDefaultFavoriteAddress()
        {
            return new DefaultFavoriteAddressResponse(_defaultAddressDao.GetAll());
        }

        [HttpPost, Route("addresses"), NoCache]
        public object AddDefaultFavoriteAddress(DefaultFavoriteAddress request)
        {
            var command = new AddDefaultFavoriteAddress();

            Mapper.Map(request, command);
            command.Address.Id = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id;

            _commandBus.Send(command);

            return new { command.Address.Id };
        }

        [HttpPut, Route("addresses/{id}"), NoCache]
        public HttpResponseMessage UpdateDefaultFavoriteAddress(Guid id, DefaultFavoriteAddress request)
        {
            var command = new UpdateDefaultFavoriteAddress();

            Mapper.Map(request, command);
            command.Address.Id = request.Id;

            _commandBus.Send(command);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpDelete, Route("addresses/{id}"), NoCache]
        public HttpResponseMessage DeleteDefaultFavoriteAddress(Guid id)
        {
            var command = new RemoveDefaultFavoriteAddress
            {
                Id = Guid.NewGuid(),
                AddressId = id
            };

            _commandBus.Send(command);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }



    }
}
