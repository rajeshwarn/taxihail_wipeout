using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using apcurium.MK.Common.Extensions;
using ServiceStack.ServiceInterface.ServiceModel;

namespace apcurium.MK.Common.Http.Exceptions
{
    public class ServiceResponseException : Exception
    {
        public ServiceResponseException(HttpResponseMessage response, string responseBody)
            : base(response.ReasonPhrase)
        {
            StatusCode = response.StatusCode;
            Response = response;
            ResponseBody = responseBody;
        }

        public HttpStatusCode StatusCode { get; private set; }

        public HttpResponseMessage Response { get; private set; }

        public string ResponseBody { get; private set; }

        public string ErrorCode
        {
            get
            {
                var error = ResponseBody.FromJsonSafe<ErrorResponse>();

                if (error.ResponseStatus == null)
                {
                    return error.ErrorCodes != null
                        ? error.ErrorCodes.FirstOrDefault()
                        : Message;
                }

                return error.ResponseStatus.ErrorCode??Message;
            }
        }

        //public string[] ErrorCodes
        //{
        //    get { var error; }
        //}

        public string ErrorMessage
        {
            get
            {
                var error = ResponseBody.FromJsonSafe<ErrorResponse>();

                return error.ResponseStatus == null
                    ? Message
                    : error.ResponseStatus.Message ?? Message;
            }
        }

        public bool IsClientError
        {
            get
            {
                return (int)StatusCode >= 400 && (int)StatusCode <= 499;
            }
        }

        public bool IsServerError
        {
            get
            {
                return (int)StatusCode >= 500 && (int)StatusCode <= 599;
            }
        }
    }
}
