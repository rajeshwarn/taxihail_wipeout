using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Controllers
{
    public class TaxihailApiHttpHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestUrl = request.RequestUri.OriginalString;

            if (!requestUrl.Contains("/api/v2/"))
            {
                // We need to update the Url to the new version.
                requestUrl = RequestUrlHelper.UpdateRequestUrl(requestUrl);
            }

            var queryStrings = request.RequestUri.Query.HasValueTrimmed() ?
                request.RequestUri.Query.Remove(0, 1).Split('&')
                : new string[0];

            if (queryStrings.Any(q => q == "format=json"))
            {
                requestUrl = requestUrl.Remove(requestUrl.IndexOf("?", StringComparison.Ordinal));
                var query = queryStrings
                    .Select(q => q.Split('=').Select(item => "\""+item+"\"").JoinBy(":"))
                    .JoinBy(",");

                query = "{" + query + "}";

                request.Content = new StringContent(query, Encoding.Default, "application/json");
            }


            request.RequestUri = new Uri(requestUrl);

            var response = await base.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return response;
            }


            return response;
        }

    }
}
