using apcurium.MK.Booking.Api.Contract.Resources;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class ReferenceDataServiceClient : BaseServiceClient
    {
        public ReferenceDataServiceClient(string url, string sessionId, string userAgent)
            : base(url, sessionId,userAgent)
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