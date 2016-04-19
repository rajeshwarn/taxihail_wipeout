

using apcurium.MK.Common.Http;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/addressFromPostalCode", "POST")]
    public class GetAddressesFromPostcalCodeRequest
    {
        public string PostalCode { get; set; }
    }
}
