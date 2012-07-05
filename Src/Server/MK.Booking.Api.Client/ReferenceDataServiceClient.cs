using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Client
{
    public class ReferenceDataServiceClient : BaseServiceClient
    {
        public ReferenceDataServiceClient(string url, AuthInfo credential)
            : base(url, credential)
        {
        }

        public ReferenceData GetReferenceData()
        {
            var result = Client.Get<ReferenceData>("/referencedata");
            return result;
        }
    }
}