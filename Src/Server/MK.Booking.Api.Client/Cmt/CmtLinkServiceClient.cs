using apcurium.MK.Booking.Api.Contract.Resources.Cmt;

namespace apcurium.MK.Booking.Api.Client.Cmt
{
    public class CmtLinkServiceClient : CmtBaseServiceClient
    {
        public CmtLinkServiceClient(string url) : base(url)
        {
        }

        public CmtLinkServiceClient(string url, CmtAuthCredentials cmtAuthCredentials) : base(url, cmtAuthCredentials)
        {
        }

        public LinkData Link(string linkCode)
        {
            var req = string.Format("/link/{0}?linkCode={1}", Credentials.AccountId, linkCode);
            var linkData = Client.Get<LinkData>(req);
            return linkData;
        }
    }
}