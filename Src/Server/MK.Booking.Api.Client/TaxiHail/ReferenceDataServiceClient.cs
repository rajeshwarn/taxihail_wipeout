#region

using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class ReferenceDataServiceClient : BaseServiceClient
    {
        public ReferenceDataServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }

        public Task<ReferenceData> GetReferenceData()
        {
            var tcs = new TaskCompletionSource<ReferenceData>();
            Client.GetAsync<ReferenceData>("/referencedata", tcs.SetResult, (result, error) => tcs.SetException(error));
            return tcs.Task;
        }
    }
}