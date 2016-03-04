using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;

namespace apcurium.MK.Web.Controllers.Api
{
    public class PopularAddressController : BaseApiController
    {
        private readonly ClientPopularAddressService _clientPopularAddressService;

        public PopularAddressController(IPopularAddressDao popularAddressDao)
        {
            _clientPopularAddressService = new ClientPopularAddressService(popularAddressDao);
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            PrepareApiServices(_clientPopularAddressService);
        }

        [HttpGet, NoCache, Route("popularaddresses")]
        public IHttpActionResult GetClientPopularAddress()
        {
            var result = _clientPopularAddressService.Get(new ClientPopularAddress());
            
            return GenerateActionResult(result);
        }

        [HttpGet, NoCache, Route("admin/popularaddresses")]
        public IHttpActionResult GetAdminPopularAddress()
        {
            var result = _clientPopularAddressService.Get(new AdminPopularAddress());

            return GenerateActionResult(result);
        }
    }
}
