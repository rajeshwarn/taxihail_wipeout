using System;
using System.Net;
using System.Net.Http;

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
