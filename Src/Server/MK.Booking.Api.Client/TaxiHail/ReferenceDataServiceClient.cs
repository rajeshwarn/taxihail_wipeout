using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class ReferenceDataServiceClient : BaseServiceClient, IReferenceDataServiceClient
    {
        public ReferenceDataServiceClient(string url, string sessionId)
            : base(url, sessionId)
        {
        }

        public ReferenceData GetReferenceData()
        {
            var result = Client.Get<ReferenceData>("/referencedata");
            return result;
        }
    }
}