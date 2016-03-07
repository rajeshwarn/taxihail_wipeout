using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract;

namespace apcurium.MK.Web.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/v2/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "Legacy",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional },
                constraints: null,
                handler: new LegacyHttpClientHandler()
            );

            // Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
            // To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
            // For more information, visit http://go.microsoft.com/fwlink/?LinkId=279712.
            //config.EnableQuerySupport();

            // To disable tracing in your application, please comment out or remove the following line of code
            // For more information, refer to: http://www.asp.net/web-api
            //config.EnableSystemDiagnosticsTracing();
        }


        private class LegacyHttpClientHandler : HttpClientHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (request.RequestUri.OriginalString.Contains("api/v2"))
                {
                    return base.SendAsync(request, cancellationToken);
                }

                var requestUrl = request.RequestUri.OriginalString.Replace("api", "api/v2");

                if (requestUrl.Contains("api/v2/auth"))
                {
                    requestUrl = requestUrl.Replace("auth", "login")
                            .Replace("credentialsfb", "facebook")
                            .Replace("credentialstw", "twitter")
                            .Replace("credentials", "password");
                }

                if (requestUrl.Contains("api/v2/encryptedsettings"))
                {
                    requestUrl = requestUrl.Replace("encryptedsettings", "settings/encrypted");

                }

                if (requestUrl.Contains("account/manualridelinq") && (requestUrl.EndsWith("/status") || requestUrl.EndsWith("/unpair") || requestUrl.EndsWith("/pair")))
                {
                    requestUrl = requestUrl
                        .Replace("/status", "")
                        .Replace("/unpair", "")
                        .Replace("/pair", "")
                        .Replace("/pairing/tip", "/tip");

                }

                if (requestUrl.Contains("account/"))
                {
                    requestUrl = requestUrl.Replace("account/", "accounts/");
                }

                request.RequestUri = new Uri(requestUrl);

                return base.SendAsync(request, cancellationToken);
            }

        }
    }
}