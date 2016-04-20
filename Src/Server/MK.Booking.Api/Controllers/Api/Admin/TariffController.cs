using System;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    public class TariffController : BaseApiController
    {
        public TariffsService TariffsService { get; private set; }

        public TariffController(TariffsService tariffsService)
        {
            TariffsService = tariffsService;
        }

        [HttpGet, Auth, Route("api/admin/tariffs")]
        public IHttpActionResult GetTariffs()
        {
            var result = TariffsService.Get();

            return GenerateActionResult(result);
        }

        [HttpPost, Auth(Role = RoleName.Admin), Route("api/admin/tariffs")]
        public IHttpActionResult CreateTariff([FromBody] Tariff request)
        {
            var result = TariffsService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPut, Route("api/admin/tariffs/{id}"),Auth(Role = RoleName.Admin)]
        public IHttpActionResult UpdateTariff(Guid id,[FromBody] Tariff request)
        {
            request.Id = id;

            TariffsService.Put(request);

            return Ok();
        }

        [HttpDelete, Route("api/admin/tariffs/{id}"), Auth(Role = RoleName.Admin)]
        public IHttpActionResult DeleteTarrif(Guid id)
        {
            TariffsService.Delete(id);

            return Ok();
        }
    }
}

