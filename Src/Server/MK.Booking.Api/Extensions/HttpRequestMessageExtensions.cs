using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web;
using System.Web.Http.Controllers;

namespace apcurium.MK.Common.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        private const string HttpContext = "MS_HttpContext";
        private const string RemoteEndpointMessageProperty ="System.ServiceModel.Channels.RemoteEndpointMessageProperty";

        public static string GetIpAddress(this HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey(HttpContext))
            {
                var context = request.Properties[HttpContext] as HttpContextWrapper;

                return context.SelectOrDefault(ctx => ctx.Request.UserHostAddress);
            }

            if (request.Properties.ContainsKey(RemoteEndpointMessageProperty))
            {
                var context = request.Properties[HttpContext] as RemoteEndpointMessageProperty;

                return context.SelectOrDefault(ctx => ctx.Address);
            }

            return null;
        }

        public static string GetBaseUrl(this HttpRequestMessage request)
        {
            var uri = request.RequestUri;

            return request.RequestUri.AbsoluteUri
                .Replace(uri.AbsoluteUri, string.Empty)
                .Replace(uri.Query, string.Empty);
        }

        public static string GetUserAgent(this HttpRequestMessage request)
        {
            if (!request.Headers.Contains("User-Agent"))
            {
                return null;
            }

            return request.Headers.GetValues("User-Agent").JoinBy(";");
        }
    }
}
