#region

using apcurium.MK.Booking.Api.Contract.Http;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.ServiceModel;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/admin/exclusions", "GET, POST")]
    public class ExclusionsRequest
    {
        public int[] ExcludedVehicleTypeId { get; set; }
        public int[] ExcludedProviderId { get; set; }
    }

    [NoCache]
    public class ExclusionsRequestResponse : IHasResponseStatus
    {
        public ExclusionsRequestResponse()
        {
            ResponseStatus = new ResponseStatus();
        }

        public int[] ExcludedVehicleTypeId { get; set; }
        public int[] ExcludedProviderId { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}