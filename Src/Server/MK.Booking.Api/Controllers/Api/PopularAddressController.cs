using System;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api
{
    public class PopularAddressController : BaseApiController
    {
        public ClientPopularAddressService ClientPopularAddressService { get; private set; }
        public PopularAddressService PopularAddressService { get; private set; }

        public PopularAddressController(ClientPopularAddressService clientPopularAddressService, PopularAddressService popularAddressService)
        {
            ClientPopularAddressService = clientPopularAddressService;
            PopularAddressService = popularAddressService;
        }

        [HttpGet, NoCache, Route("api/v2/popularaddresses")]
        public IHttpActionResult GetClientPopularAddress()
        {
            var result = ClientPopularAddressService.Get(new ClientPopularAddress());
            
            return GenerateActionResult(result);
        }

        [HttpGet, NoCache, Route("api/v2/admin/popularaddresses")]
        public IHttpActionResult GetAdminPopularAddress()
        {
            var result = ClientPopularAddressService.Get(new AdminPopularAddress());

            return GenerateActionResult(result);
        }

        [HttpPost, Auth(Role = RoleName.Support), Route("api/v2/admin/popularaddresses")]
        public IHttpActionResult CreatePopularAddress([FromBody] PopularAddress request)
        {
            var result = PopularAddressService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpDelete, Auth(Role = RoleName.Support), Route("api/v2/admin/popularaddresses/{id}")]
        public IHttpActionResult DeletePopularAddress(Guid id)
        {
            PopularAddressService.Delete(id);

            return Ok();
        }

        [HttpPut, Auth(Role = RoleName.Support), Route("api/v2/admin/popularaddresses/{id}")]
        public IHttpActionResult UpdatePopularAddress(Guid id, [FromBody] PopularAddress request)
        {
            request.Id = id;

            PopularAddressService.Put(request);

            return Ok();
        }
    }
}
