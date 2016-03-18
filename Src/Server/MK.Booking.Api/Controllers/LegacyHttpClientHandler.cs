using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Api.Controllers
{
    public class LegacyHttpClientHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri.OriginalString.Contains("api/v2"))
            {
                return await base.SendAsync(request, cancellationToken);
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

            if (requestUrl.Contains("/payments/settleoverduepayment"))
            {
                requestUrl = requestUrl.Replace("/payments/settleoverduepayment", "/accounts/settleoverduepayment");
            }

            if (requestUrl.Contains("account/"))
            {
                requestUrl = requestUrl.Replace("account/", "accounts/");
            }

            if (requestUrl.Contains("creditCard/"))
            {
                requestUrl = requestUrl.Replace("creditCard/", "creditCards/");
            }

            if (requestUrl.Contains("/ordercountforapprating"))
            {
                requestUrl = requestUrl.Replace("/ordercountforapprating", "/orders/countforapprating");
            }

            request.RequestUri = new Uri(requestUrl);

            var responseMessage = await base.SendAsync(request, cancellationToken);

            if (!responseMessage.IsSuccessStatusCode)
            {
                return responseMessage;
            }

            return responseMessage;
        }

    }
}
