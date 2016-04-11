using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;

namespace apcurium.MK.Web.Controllers.Api.Account.Orders
{
    public class ValidateOrderController : BaseApiController
    {
        public ValidateOrderService ValidateOrderService { get; private set; }

        public ValidateOrderController(ValidateOrderService validateOrderService)
        {
            ValidateOrderService = validateOrderService;
        }

        [HttpPost, Route("api/v2/accounts/orders/validate")]
        public async Task<IHttpActionResult> ValidateOrder([FromBody]ValidateOrderRequest request)
        {
            var result  = await ValidateOrderService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Route("api/v2/accounts/orders/validate/{forError}")]
        public Task<IHttpActionResult> ValidateOrderForError(bool forError, [FromBody] ValidateOrderRequest request)
        {
            request.ForError = forError;

            return ValidateOrder(request);
        }

        [HttpPost, Route("api/v2/accounts/orders/validate/{forError}/{testZone}")]
        public Task<IHttpActionResult> ValidateOrderForError(bool forError, string testZone, [FromBody] ValidateOrderRequest request)
        {
            request.ForError = forError;
            request.TestZone = testZone;

            return ValidateOrder(request);
        }

    }
}
