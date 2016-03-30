﻿using System;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    public class TariffController : BaseApiController
    {
        public TariffsService TariffsService { get; }

        public TariffController(ITariffDao tariffDao, ICommandBus commandBus)
        {
            TariffsService = new TariffsService(tariffDao, commandBus);
        }

        [HttpGet, Auth, Route("api/v2/admin/tariffs")]
        public IHttpActionResult GetTariffs()
        {
            var result = TariffsService.Get();

            return GenerateActionResult(result);
        }

        [HttpPost, Auth(Role = RoleName.Admin), Route("api/v2/admin/tariffs")]
        public IHttpActionResult CreateTariff([FromBody] Tariff request)
        {
            var result = TariffsService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPut, Route("api/v2/admin/tariffs/{id}"),Auth(Role = RoleName.Admin)]
        public IHttpActionResult UpdateTariff(Guid id,[FromBody] Tariff request)
        {
            request.Id = id;

            TariffsService.Put(request);

            return Ok();
        }

        [HttpDelete, Route("api/v2/admin/tariffs/{id}"), Auth(Role = RoleName.Admin)]
        public IHttpActionResult DeleteTarrif(Guid id)
        {
            TariffsService.Delete(id);

            return Ok();
        }
    }
}
