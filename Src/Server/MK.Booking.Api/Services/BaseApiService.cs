using System;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Http;
using ServiceStack.ServiceInterface.ServiceModel;

namespace apcurium.MK.Booking.Api.Services
{
    public class BaseApiService
    {
        public SessionEntity Session { get; set; } = new SessionEntity();

        public HttpRequestContext HttpRequestContext { get; set; }

        public HttpRequestMessage HttpRequest { get; set; }


        protected HttpResponseException GenerateException(HttpStatusCode statusCode, string errorCode, string errorMessage, string stackTrace = null,[CallerMemberName] string memberName = null, [CallerLineNumber] int line = -1)
        {
            var exceptionContent = new ErrorResponse
            {
                ResponseStatus = new ResponseStatus()
                {
                    ErrorCode = errorCode,
                    Message = errorMessage,
                    StackTrace = stackTrace??"{0}:{1}".InvariantCultureFormat(memberName, line)
                }
            };

            var jsonContent = exceptionContent.ToJson();

            var exceptionMessage = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(jsonContent, Encoding.Default, "application/json")
            };

            return new HttpResponseException(exceptionMessage);
        }

    }
}
