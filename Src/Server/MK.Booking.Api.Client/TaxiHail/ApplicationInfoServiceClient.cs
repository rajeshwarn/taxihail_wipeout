using apcurium.MK.Booking.Api.Contract.Resources;
using System.Threading;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class ApplicationInfoServiceClient : BaseServiceClient
    {
        public ApplicationInfoServiceClient(string url, string sessionId, string userAgent)
            : base(url, sessionId, userAgent)
        {
        }


        public Task<ApplicationInfo> GetAppInfoAsync()
        {
            var tcs = new TaskCompletionSource<ApplicationInfo>();
            var resource = string.Format("/app/info");
            Client.GetAsync<ApplicationInfo>(resource, tcs.SetResult, (result, error) => tcs.SetException(error));
            return tcs.Task;
        }
    
    }
}
