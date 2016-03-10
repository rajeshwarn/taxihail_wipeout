using System;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api
{
    [RoutePrefix("api/v2")]
    public class PopularAddressController : BaseApiController
    {
        public ClientPopularAddressService ClientPopularAddressService { get; }
        public PopularAddressService PopularAddressService { get; set; }

        public PopularAddressController(IPopularAddressDao popularAddressDao, ICommandBus commandBus)
        {
            ClientPopularAddressService = new ClientPopularAddressService(popularAddressDao);
            PopularAddressService = new PopularAddressService(popularAddressDao, commandBus);
        }

        [HttpGet, NoCache, Route("popularaddresses")]
        public IHttpActionResult GetClientPopularAddress()
        {
            var result = ClientPopularAddressService.Get(new ClientPopularAddress());
            
            return GenerateActionResult(result);
        }

        [HttpGet, NoCache, Route("admin/popularaddresses")]
        public IHttpActionResult GetAdminPopularAddress()
        {
            var result = ClientPopularAddressService.Get(new AdminPopularAddress());

            return GenerateActionResult(result);
        }

        [HttpPost, Auth(Role = RoleName.Support), Route("admin/popularaddresses")]
        public IHttpActionResult CreatePopularAddress([FromBody] PopularAddress request)
        {
            var result = PopularAddressService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpDelete, Auth(Role = RoleName.Support), Route("admin/popularaddresses/{id}")]
        public IHttpActionResult DeletePopularAddress(Guid id)
        {
            PopularAddressService.Delete(id);

            return Ok();
        }

        [HttpPut, Auth(Role = RoleName.Support), Route("admin/popularaddresses/{id}")]
        public IHttpActionResult UpdatePopularAddress(Guid id, [FromBody] PopularAddress request)
        {
            request.Id = id;

            PopularAddressService.Put(request);

            return Ok();
        }
    }
}
