using System;
using System.Net.Http;
using apcurium.MK.Common.Extensions;

namespace CMTPayment
{
    public partial class BaseServiceClient
    {
        private HttpClient CreateClient()
        {
            var uri = new Uri(_url);

            var client = new HttpClient()
            {
                BaseAddress = uri,
                Timeout = new TimeSpan(0, 0, 2, 0, 0)
            };

            // When packageInfo is not specified, we use a default value as the useragent
            client.DefaultRequestHeaders.Add("User-Agent", GetUserAgent());
            if (_packageInfo != null)
            {
                client.DefaultRequestHeaders.Add("ClientVersion", _packageInfo.Version);
            }

            if (_sessionId.HasValueTrimmed())
            {
                client.DefaultRequestHeaders.Add("Cookie", "ss-opt=perm; ss-pid=" + _sessionId);
            }
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            client.DefaultRequestHeaders.AcceptCharset.ParseAdd("utf-8");
            client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip");
            client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("deflate");

            return client;
        }
    }
}
