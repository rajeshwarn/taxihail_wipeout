using System;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api
{
    [RoutePrefix("ratingtypes")]
    public class RatingTypesController : BaseApiController
    {
        public RatingTypesService RatingTypesService { get; set; }

        public RatingTypesController(IRatingTypeDao ratingTypeDao, ICommandBus commandBus)
        {
            RatingTypesService = new RatingTypesService(ratingTypeDao, commandBus);
        }

        [HttpGet, Auth]
        public IHttpActionResult GetAllRatingTypes()
        {
            var result = RatingTypesService.Get(new RatingTypesRequest());

            return GenerateActionResult(result);
        }

        [HttpGet, Auth, Route("{clientLanguage}")]
        public IHttpActionResult GetRatingTypesByLanguage(string clientLanguage)
        {
            var result = RatingTypesService.Get(new RatingTypesRequest() {ClientLanguage = clientLanguage});

            return GenerateActionResult(result);
        }

        [HttpPost, Auth(Role = RoleName.Admin)]
        public IHttpActionResult CreateRatingType([FromBody]RatingTypesRequest request)
        {
            var result = RatingTypesService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPut, Auth(Role = RoleName.Admin)]
        public IHttpActionResult UpdateRatingType([FromBody] RatingTypesRequest request)
        {
            RatingTypesService.Put(request);

            return Ok();
        }

        [HttpDelete, Auth(Role = RoleName.Admin), Route("{ratingTypeId}")]
        public IHttpActionResult DeleteRatingType(Guid ratingTypeId)
        {
            RatingTypesService.Delete(ratingTypeId);

            return Ok();
        }
        
    }
}
