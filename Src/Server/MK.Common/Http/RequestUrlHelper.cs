﻿using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Common.Http
{
    public static class RequestUrlHelper
    {
        public static string UpdateRequestUrl(string requestUrl)
        {
            var originalUrl = requestUrl.Clone();

            requestUrl = requestUrl.Replace("api/", "api/v2/");

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

            if (requestUrl.EndsWith("/creditCard") || requestUrl.EndsWith("/account"))
            {
                requestUrl = requestUrl.Append("s");
            }

            requestUrl = requestUrl
                    .Replace("/account/grantadmin", "/admin/grantadmin")
                    .Replace("/account/grantsupport", "/admin/grantsupport")
                    .Replace("/account/grantsuperadmin", "/admin/grantsuperadmin")
                    .Replace("/account/revokeaccess", "/admin/revokeaccess")
                    .Replace("/payments/settleoverduepayment", "/accounts/settleoverduepayment")
                    .Replace("account/", "accounts/")
                    .Replace("creditCard/", "creditCards/")
                    .Replace("/order/logeta", "/orders/logeta")
                    .Replace("/ordercountforapprating", "/orders/countforapprating");

            return requestUrl;
        }
    }
}
