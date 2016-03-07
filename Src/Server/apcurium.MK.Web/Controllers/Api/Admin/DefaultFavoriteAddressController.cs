using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
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
    [RoutePrefix("api/v2/admin"), Auth(Role = RoleName.Support)]
    public class DefaultFavoriteAddressController : BaseApiController
    {
        private readonly DefaultFavoriteAddressService _defaultFavoriteAddressService;
        
        public DefaultFavoriteAddressController(ICommandBus commandBus, IDefaultAddressDao defaultAddressDao)
        {
            _defaultFavoriteAddressService = new DefaultFavoriteAddressService(defaultAddressDao, commandBus);
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            PrepareApiServices(_defaultFavoriteAddressService);
        }

        [HttpGet, Route("addresses"), NoCache]
        public IHttpActionResult GetDefaultFavoriteAddress()
        {
            var result = _defaultFavoriteAddressService.Get();

            return GenerateActionResult(result);
        }

        [HttpPost, Route("addresses"), NoCache]
        public IHttpActionResult AddDefaultFavoriteAddress([FromBody]DefaultFavoriteAddress request)
        {
            var result = _defaultFavoriteAddressService.Post(request);

            return GenerateActionResult(new {Id = result});
        }

        [HttpPut, Route("addresses/{id}"), NoCache]
        public IHttpActionResult UpdateDefaultFavoriteAddress(Guid id, [FromBody]DefaultFavoriteAddress request)
        {
            request.Id = id;

            _defaultFavoriteAddressService.Put(request);

            return Ok();
        }

        [HttpDelete, Route("addresses/{id}"), NoCache]
        public IHttpActionResult DeleteDefaultFavoriteAddress(Guid id)
        {
            _defaultFavoriteAddressService.Delete(new DefaultFavoriteAddress() {Id = id});
            
            return Ok();
        }



    }
}
