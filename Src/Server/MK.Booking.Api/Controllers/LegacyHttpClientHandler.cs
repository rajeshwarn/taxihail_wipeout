using System;
using System.Net.Http;
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

            requestUrl = requestUrl
                    .Replace("account/grantadmin", "admin/grantadmin")
                    .Replace("account/grantsupport", "admin/grantsupport")
                    .Replace("account/grantsuperadmin", "admin/grantsuperadmin")
                    .Replace("account/revokeaccess", "admin/revokeaccess")
                    .Replace("/payments/settleoverduepayment", "/accounts/settleoverduepayment")
                    .Replace("account/", "accounts/")
                    .Replace("creditCard/", "creditCards/")
                    .Replace("/ordercountforapprating", "/orders/countforapprating");

            request.RequestUri = new Uri(requestUrl);

            return await base.SendAsync(request, cancellationToken);
        }

    }
}
