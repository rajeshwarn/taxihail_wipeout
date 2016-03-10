using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Admin;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    [RoutePrefix("api/v2/admin/exclusions")]
    public class ExclusionsController : BaseApiController
    {
        public ExclusionsService ExclusionsService { get; }

        public ExclusionsController(IServerSettings serverSettings, ICommandBus commandBus, ICacheClient cacheClient)
        {
            ExclusionsService = new ExclusionsService(serverSettings, commandBus, cacheClient);
        }

        [HttpGet, Auth]
        public IHttpActionResult GetExclusions()
        {
            var result = ExclusionsService.Get();

            return GenerateActionResult(result);
        }

        [HttpPost, Auth]
        public IHttpActionResult UpdateExclusions([FromBody]ExclusionsRequest request)
        {
            ExclusionsService.Post(request);

            return Ok();
        }
    }
}
