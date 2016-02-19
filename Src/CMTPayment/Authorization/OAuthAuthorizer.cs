using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using apcurium.MK.Common.Extensions;
using CMTPayment.Utilities;

namespace CMTPayment.Authorization
{
    public class OAuthAuthorizer
    {
        private static readonly ThreadSafeRandom Random = new ThreadSafeRandom();
        private static readonly DateTime UnixBaseTime = new DateTime(1970, 1, 1);

        public static string AuthorizeRequest(string consumerKey, string consumerKeySecret, string oauthToken,
            string oauthTokenSecret, string method, Uri uri, string data)
        {
            var headers = new Dictionary<string, string>
            {
                {"oauth_consumer_key", consumerKey},
                {"oauth_nonce", MakeNonce()},
                {"oauth_signature_method", "HMAC-SHA1"},
                {"oauth_timestamp", MakeTimestamp()},
                {"oauth_version", "1.0"}
            };
            var signatureHeaders = new Dictionary<string, string>(headers);

            if (!string.IsNullOrEmpty(oauthToken))
            {
                signatureHeaders.Add("oauth_token", oauthToken);
            }

            // Add the data and URL query string to the copy of the headers for computing the signature
            if (!string.IsNullOrEmpty(data))
            {
                var parsed = HttpUtility.ParseQueryString(data);
                foreach (HttpValue httpValue in parsed)
                {
                    signatureHeaders.Add(httpValue.Key, OAuthUtils.PercentEncode(httpValue.Value));
                }
            }

            var nvc = HttpUtility.ParseQueryString(uri.Query);
            foreach (var key in nvc.Cast<string>().Where(key => key != null))
            {
                signatureHeaders.Add(key, OAuthUtils.PercentEncode(nvc[key]));
            }

            var signature = MakeSignature(method, uri.GetLeftPart(UriPartial.Path), signatureHeaders);
            var compositeSigningKey = MakeSigningKey(consumerKeySecret, oauthTokenSecret);
            var oauthSignature = MakeOAuthSignature(compositeSigningKey, signature);

            headers.Add("oauth_signature", OAuthUtils.PercentEncode(oauthSignature));

            return HeadersToOAuth(headers);
        }

        private static string MakeNonce()
        {
            var ret = new char[16];
            for (var i = 0; i < ret.Length; i++)
            {
                var n = Random.Next(35);
                if (n < 10)
                {
                    ret[i] = (char) (n + '0');
                }
                else
                {
                    ret[i] = (char)(n - 10 + 'a');
                }
                    
            }
            return new string(ret);
        }

        private static string MakeTimestamp()
        {
            return ((long) (DateTime.UtcNow - UnixBaseTime).TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        private static string MakeSignature(string method, string baseUri, Dictionary<string, string> headers)
        {
            var items = from k in headers.Keys
                orderby k
                select k + "%3D" + OAuthUtils.PercentEncode(headers[k]);

            return method + "&" + OAuthUtils.PercentEncode(baseUri) + "&" +
                   string.Join("%26", items.ToArray());
        }

        private static string MakeSigningKey(string consumerSecret, string oauthTokenSecret)
        {
            return OAuthUtils.PercentEncode(consumerSecret) + "&" +
                   (oauthTokenSecret != null ? OAuthUtils.PercentEncode(oauthTokenSecret) : "");
        }

        private static string MakeOAuthSignature(string compositeSigningKey, string signatureBase)
        {
            var sha1 = new HMACSHA1(Encoding.UTF8.GetBytes(compositeSigningKey));

            return Convert.ToBase64String(sha1.ComputeHash(Encoding.UTF8.GetBytes(signatureBase)));
        }

        private static string HeadersToOAuth(Dictionary<string, string> headers)
        {
            var oAuthContents = headers.Select(h => string.Format("{0}=\"{1}\"", h.Key, h.Value));

            return "OAuth " + oAuthContents.JoinBy(",");
        }
    }
}