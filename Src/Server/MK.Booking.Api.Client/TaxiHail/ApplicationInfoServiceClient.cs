using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class ApplicationInfoServiceClient : BaseServiceClient
    {
        public ApplicationInfoServiceClient(string url, string sessionId)
            : base(url, sessionId)
        {
        }


        public ApplicationInfo GetAppInfo()
        {
            var resource = string.Format("/app/info");
            var result = Client.Get<ApplicationInfo>(resource);
            return result;
        }
    
    }
}
