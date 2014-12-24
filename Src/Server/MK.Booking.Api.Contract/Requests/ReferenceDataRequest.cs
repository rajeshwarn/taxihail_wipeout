#region

using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/referencedata", "GET")]
    public class ReferenceDataRequest : BaseDto
    {
        public bool WithoutFiltering { get; set; }

        public string CompanyKey { get; set; }

        public string Market { get; set; }
    }
}