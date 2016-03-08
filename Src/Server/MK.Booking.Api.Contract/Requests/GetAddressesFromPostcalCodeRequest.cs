using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/addressFromPostalCode", "POST")]
    public class GetAddressesFromPostcalCodeRequest
    {
        public string PostalCode { get; set; }
    }
}
