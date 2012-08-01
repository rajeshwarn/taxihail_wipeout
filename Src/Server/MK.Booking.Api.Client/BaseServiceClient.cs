using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Booking.Api.Client
{
    public class BaseServiceClient
    {
        private ServiceClientBase _client;
        protected readonly string _url;
        public BaseServiceClient(string url)
        {
            _url = url;
        }

        protected ServiceClientBase Client
        {
            get { return _client ?? (_client = new JsonServiceClient(_url)); }
        }



    }
}
