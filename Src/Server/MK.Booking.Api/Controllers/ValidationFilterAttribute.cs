using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using apcurium.MK.Common.Extensions;
using ServiceStack.ServiceInterface.ServiceModel;
using ActionFilterAttribute = System.Web.Http.Filters.ActionFilterAttribute;

namespace apcurium.MK.Booking.Api.Contract.Controllers
{
    public class ValidationFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!actionContext.ModelState.IsValid)
            {
                actionContext.Response = CreateErrorResponse(actionContext.ModelState);
            }
        }


        private HttpResponseMessage CreateErrorResponse(ModelStateDictionary modelState)
        {
            var errors = modelState.Values
                .SelectMany(c => c.Errors)
                .Select(c => c.ErrorMessage)
                .Distinct()
                .ToArray();

            var error = new ErrorResponse
            {
                ValidationResponseStatus = new ValidationResponseStatus()
                {
                    ValidationErrorCodes = errors
                },
                ResponseStatus = new ResponseStatus
                {
                    ErrorCode = errors.FirstOrDefault()
                }
            };
            
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(error.ToJson(), Encoding.UTF8, "application/json")
            };
        }
    }
}
