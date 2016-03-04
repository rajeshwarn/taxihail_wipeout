using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Http;
using Microsoft.Practices.Unity;
using UnityContainer = apcurium.MK.Common.IoC.UnityServiceLocator;

namespace apcurium.MK.Booking.Api.Services
{
    public class BaseApiController : ApiController
    {
        private readonly ICacheClient _cacheClient = UnityContainer.Instance.Resolve<ICacheClient>();

        protected HttpException GetException(HttpStatusCode statusCode, string message)
        {
            return new HttpException((int)statusCode, message);
        }


        public IHttpActionResult GenerateActionResult<T>(T content)
        {
            return Ok(content);
        }

        protected void PrepareApiServices(params BaseApiService[] targets)
        {
            if (targets == null || targets.None())
            {
                return;
            }

            foreach (var baseApiService in targets)
            {
                baseApiService.Session = Session;
                baseApiService.HttpRequest = Request;
                baseApiService.HttpRequestContext = RequestContext;
            }

            
        }

        protected ILogger Logger { get; set; } = UnityContainer.Instance.Resolve<ILogger>();

        public SessionEntity Session
        {
            get
            {
                var sessionId = Request.Headers.Where(request =>
                    request.Key.Equals("Cookie", StringComparison.InvariantCultureIgnoreCase) &&
                    request.Value.Any(val => val == "ss-opt=perm")
                )
                .Select(request => request.Value.FirstOrDefault(p => p.StartsWith("ss-pid")))
                .Select(pid => pid.Split('=').Last())
                .FirstOrDefault();

                if (!sessionId.HasValueTrimmed())
                {
                    return new SessionEntity();
                }

                var urn = "urn:iauthsession:{0}".InvariantCultureFormat(sessionId);

                var cachedSession = _cacheClient.Get<SessionEntity>(urn);

                return cachedSession;
            }
        }
    }
}
