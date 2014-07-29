#region

using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.ServiceModel;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [AuthorizationRequired(ApplyTo.Post, RoleName.Admin)]
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