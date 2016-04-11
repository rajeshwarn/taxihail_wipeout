using System;
using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    [Auth(Role = RoleName.Support)]
    public class DefaultFavoriteAddressController : BaseApiController
    {
        public DefaultFavoriteAddressService FavoriteAddressService { get; private set; }

        public DefaultFavoriteAddressController(DefaultFavoriteAddressService favoriteAddressService)
        {
            FavoriteAddressService = favoriteAddressService;
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            PrepareApiServices(FavoriteAddressService);
        }

        [HttpGet, Route("api/v2/admin/addresses"), NoCache]
        public IHttpActionResult GetDefaultFavoriteAddress()
        {
            var result = FavoriteAddressService.Get();

            return GenerateActionResult(result);
        }

        [HttpPost, Route("api/v2/admin/addresses"), NoCache]
        public IHttpActionResult AddDefaultFavoriteAddress([FromBody]DefaultFavoriteAddress request)
        {
            var result = FavoriteAddressService.Post(request);

            return GenerateActionResult(new {Id = result});
        }

        [HttpPut, Route("api/v2/admin/addresses/{id}"), NoCache]
        public IHttpActionResult UpdateDefaultFavoriteAddress(Guid id, [FromBody]DefaultFavoriteAddress request)
        {
            request.Id = id;

            FavoriteAddressService.Put(request);

            return Ok();
        }

        [HttpDelete, Route("api/v2/admin/addresses/{id}"), NoCache]
        public IHttpActionResult DeleteDefaultFavoriteAddress(Guid id)
        {
            FavoriteAddressService.Delete(new DefaultFavoriteAddress() {Id = id});
            
            return Ok();
        }



    }
}
