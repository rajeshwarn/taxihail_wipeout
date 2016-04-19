#region


#endregion

using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/referencedata", "GET")]
    public class ReferenceDataRequest : BaseDto
    {
        public bool WithoutFiltering { get; set; }

        public string CompanyKey { get; set; }
    }

    [RouteDescription("/references/{ListName}", "GET")]
    [RouteDescription("/references/{ListName}/{SearchText}", "GET")]
    public class ReferenceListRequest : BaseDto
    {
        public string ListName { get; set; }
        public string SearchText { get; set; }
        public bool coreFieldsOnly { get; set; }
        public int size { get; set; }
    }

}