using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.ServiceModel;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [AuthorizationRequired(ApplyTo.Post, Permissions.Admin)]
    [RestService("/admin/exclusions", "GET, POST")]
    public class ExclusionsRequest
    {
        public int[] ExcludedVehicleTypeId { get; set; }
        public int[] ExcludedPaymentTypeId { get; set; }
        public int[] ExcludedProviderId { get; set; }
    }

    [NoCache]
    public class ExclusionsRequestResponse: IHasResponseStatus
    {
        public ExclusionsRequestResponse()
        {
            ResponseStatus = new ResponseStatus();
        }

        public ResponseStatus ResponseStatus { get; set; }

        public int[] ExcludedVehicleTypeId { get; set; }
        public int[] ExcludedPaymentTypeId { get; set; }
        public int[] ExcludedProviderId { get; set; }
    }
}