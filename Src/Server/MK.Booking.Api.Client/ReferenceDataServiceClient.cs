using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Client
{
    public class ReferenceDataServiceClient : BaseServiceClient
    {
        public ReferenceDataServiceClient(string url)
            : base(url)
        {
        }

        public ReferenceData GetReferenceData()
        {
            var result = Client.Get<ReferenceData>("/referencedata");
            return result;
        }
    }
}