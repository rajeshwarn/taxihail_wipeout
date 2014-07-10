#region

using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class ApplicationInfoServiceClient : BaseServiceClient
    {
        public ApplicationInfoServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
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