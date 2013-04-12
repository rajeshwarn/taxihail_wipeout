using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Booking.Api.Client.Client
{
    public class CmtJsonServiceClient : JsonServiceClient
    {
        public CmtJsonServiceClient(string url)
            : base(url)
        {

        }

        public override TResponse Send<TResponse>(object request)
        {
            var response = ((ServiceClientBase)this).Send<TResponse>(request);
            return response;
        }
    }
}