using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using apcurium.MK.Booking.Api.Extensions;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Http;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using UnityContainer = apcurium.MK.Common.IoC.UnityServiceLocator;

namespace apcurium.MK.Booking.Api.Services
{
    public class BaseApiController : ApiController
    {
        private readonly ICacheClient _cacheClient = UnityContainer.Instance.Resolve<ICacheClient>();


        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            var services = GetType()
                .GetProperties()
                .Where(p => typeof (BaseApiService).IsAssignableFrom(p.PropertyType))
                .Select(p => p.GetValue(this))
                .Cast<BaseApiService>();

            PrepareApiServices(services.ToArray());
        }

        private JsonSerializerSettings GetSerializerSettings()
        {
            return new JsonSerializerSettings()
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ContractResolver = new CustomCamelCasePropertyNamesContractResolver(),
            };
        }

        public IHttpActionResult GenerateActionResult<T>(T content)
        {
            if (content != null)
            {
                return Json(content, GetSerializerSettings());
            }

            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(string.Empty, Encoding.Default, "application/json")
            };

            return ResponseMessage(httpResponseMessage);
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


            targets.ToArray();
        }

        protected ILogger Logger { get; set; } = UnityContainer.Instance.Resolve<ILogger>();

        private string _sessionKey;
        protected string SessionKey
        {
            get
            {
                if (_sessionKey.HasValueTrimmed())
                {
                    return _sessionKey;
                }

                var sessionId = Request.Headers.GetCookies()
                    .SelectMany(cookieContainer => cookieContainer.Cookies)
                    .Where(cookie => cookie.Name == "ss-pid")
                    .Select(cookie => cookie.Value)
                    .FirstOrDefault();

                _sessionKey = sessionId.HasValueTrimmed()
                    ? "urn:iauthsession:{0}".InvariantCultureFormat(sessionId)
                    : null;

                return _sessionKey;
            }
        }

        private SessionEntity _session;
        public SessionEntity Session
        {
            get
            {
                if (_session != null)
                {
                    return _session;
                }

                var key = SessionKey;

                _session = key.HasValueTrimmed()
                    ? _cacheClient.GetOrDefault(key, new SessionEntity())
                    : new SessionEntity();

                return _session;
            }
        }

        protected void ForgetSession()
        {
            Session.RemoveSessionIfNeeded();

            _sessionKey = null;
            _session = new SessionEntity();
        }
    }
}
