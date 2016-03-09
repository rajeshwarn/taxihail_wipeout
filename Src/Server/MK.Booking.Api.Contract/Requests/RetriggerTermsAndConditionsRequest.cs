using ServiceStack.ServiceHost;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/termsandconditions/retrigger", "POST")]
    public class RetriggerTermsAndConditionsRequest : IReturn<TermsAndConditions>
    {
    }
}