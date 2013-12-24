#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

#endregion

namespace apcurium.MK.Booking.Api.Client.Cmt.OAuth
{
    public class OAuthAuthorizer
    {
        private static readonly Random Random = new Random();
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
                foreach (string k in parsed.Keys)
                {
                    signatureHeaders.Add(k, OAuthUtils.PercentEncode(parsed[k]));
                }
            }

            var nvc = HttpUtility.ParseQueryString(uri.Query);
            foreach (string key in nvc)
            {
                if (key != null)
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
                    ret[i] = (char) (n + '0');
                else
                    ret[i] = (char) (n - 10 + 'a');
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
            return "OAuth " +
                   String.Join(",",
                       (from x in headers.Keys select String.Format("{0}=\"{1}\"", x, headers[x])).ToArray());
        }
    }

    public static class OAuthUtils
    {
        // 
        // This url encoder is different than regular Url encoding found in .NET 
        // as it is used to compute the signature based on a url.   Every document
        // on the web omits this little detail leading to wasting everyone's time.
        //
        // This has got to be one of the lamest specs and requirements ever produced
        //
        public static string PercentEncode(string s)
        {
            var sb = new StringBuilder();

            foreach (var c in Encoding.UTF8.GetBytes(s))
            {
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '-' || c == '_' ||
                    c == '.' || c == '~')
                    sb.Append((char) c);
                else
                {
                    sb.AppendFormat("%{0:X2}", c);
                }
            }
            return sb.ToString();
        }
    }
}