using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Web.Controllers.Api
{
    [RoutePrefix("api/v2")]
    public class ReferenceDataController : BaseApiController
    {
        public ReferenceDataService ReferenceDataService { get; }

        public ReferenceDataController(IIBSServiceProvider ibsServiceProvider, ICacheClient cacheClient, IServerSettings serverSettings, IAirlineDao airlineDao,IPickupPointDao pickupPointDao)
        {
            ReferenceDataService = new ReferenceDataService(ibsServiceProvider, cacheClient, serverSettings, airlineDao, pickupPointDao);
        }

        [HttpGet, Route("referencedata")]
        public IHttpActionResult GetReferenceData([FromUri]ReferenceDataRequest request)
        {
            var result = ReferenceDataService.Get(request);

            return GenerateActionResult(result);
        }

        [HttpGet, Route("references/{listName}")]
        public IHttpActionResult GetReferenceList(string listName, [FromUri] ReferenceListRequest request)
        {
            request.ListName = listName;

            var result = ReferenceDataService.Get(request);

            return GenerateActionResult(result);
        }
    }
}
