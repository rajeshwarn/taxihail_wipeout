#region

using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/admin/exclusions", "GET, POST")]
    public class ExclusionsRequest
    {
        public int[] ExcludedVehicleTypeId { get; set; }
        public int[] ExcludedProviderId { get; set; }
    }

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