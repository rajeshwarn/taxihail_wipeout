using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/admin/testemail/{EmailAddress}", "POST")]
    public class TestEmailAdministrationRequest
    {
        public string EmailAddress { get; set; }
        public string TemplateName { get; set; }
        public string Language { get; set; }
    }
}