using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;

namespace apcurium.MK.Web.Controllers.Api
{
    public class ReferenceDataController : BaseApiController
    {
        public ReferenceDataService ReferenceDataService { get; private set; }

        public ReferenceDataController(ReferenceDataService referenceDataService)
        {
            ReferenceDataService = referenceDataService;
        }

        [HttpGet, Route("api/v2/referencedata")]
        public IHttpActionResult GetReferenceData([FromUri]ReferenceDataRequest request)
        {
            var result = ReferenceDataService.Get(request??new ReferenceDataRequest());

            return GenerateActionResult(result);
        }

        [HttpGet, Route("api/v2/references/{listName}/{searchText}")]
        public IHttpActionResult GetReferenceList(string listName, string searchText, [FromUri] ReferenceListRequest request)
        {
            if (request == null)
            {
                request = new ReferenceListRequest();
            }

            request.ListName = listName;
            request.SearchText = searchText;

            var result = ReferenceDataService.Get(request);

            return GenerateActionResult(result);
        }

        [HttpGet, Route("api/v2/references/{listName}")]
        public IHttpActionResult GetReferenceList(string listName, [FromUri] ReferenceListRequest request)
        {
            return GetReferenceList(listName, null, request);
        }
    }
}
