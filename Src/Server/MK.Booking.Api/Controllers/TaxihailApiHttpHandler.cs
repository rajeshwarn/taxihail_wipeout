using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Controllers
{
    public class TaxihailApiHttpHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestUrl = request.RequestUri.OriginalString;

            if (requestUrl.Contains("api/v2/auth"))
            {
                requestUrl = requestUrl.Replace("credentialsfb", "login/facebook")
                        .Replace("credentialstw", "login/twitter")
                        .Replace("credentials", "login/password");
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

            requestUrl = requestUrl
                    .Replace("account/grantadmin", "admin/grantadmin")
                    .Replace("account/grantsupport", "admin/grantsupport")
                    .Replace("account/grantsuperadmin", "admin/grantsuperadmin")
                    .Replace("account/revokeaccess", "admin/revokeaccess")
                    .Replace("/payments/settleoverduepayment", "/accounts/settleoverduepayment")
                    .Replace("account/", "accounts/")
                    .Replace("creditCard/", "creditCards/")
                    .Replace("/ordercountforapprating", "/orders/countforapprating");


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

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

    }
}
