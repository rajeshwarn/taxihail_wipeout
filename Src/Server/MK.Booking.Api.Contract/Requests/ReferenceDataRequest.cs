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
    }

    [Route("/references/{ListName}", "GET")]
    [Route("/references/{ListName}/{SearchText}", "GET")]
    public class ReferenceListRequest : BaseDto
    {
        public string ListName { get; set; }
        public string SearchText { get; set; }
        public bool coreFieldsOnly { get; set; }
        public int size { get; set; }
    }

}