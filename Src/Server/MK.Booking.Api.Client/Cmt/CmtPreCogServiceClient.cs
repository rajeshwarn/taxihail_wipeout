using System.Collections.Generic;
using System.Linq;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Requests.Cmt;
using apcurium.MK.Booking.Api.Contract.Resources.Cmt;

namespace apcurium.MK.Booking.Api.Client.Cmt
{
    public class CmtPreCogServiceClient : CmtBaseServiceClient
    {
        public CmtPreCogServiceClient(string url) : base(url)
        {
        }

        public CmtPreCogServiceClient(string url, CmtAuthCredentials cmtAuthCredentials) : base(url, cmtAuthCredentials)
        {
        }

        public PreCogResponse Send(PreCogRequest request)
        {
            var req = string.Format("/precog/{0}?{1}", Credentials.AccountId, FormatRequest(request));
            var response = Client.Get<PreCogResponse>(req);
            return response;
        }

        private string FormatRequest(PreCogRequest request)
        {
            var keyValues = request.ToStringDictionary();
            var queryString = keyValues.Skip(1).Aggregate<KeyValuePair<string, string>, string>(keyValues.First().Key + "=" + keyValues.First().Value,
                                                                      (qs, property) => qs + "&" + property.Key + "=" + property.Value);
            return queryString;
        }
    }
}