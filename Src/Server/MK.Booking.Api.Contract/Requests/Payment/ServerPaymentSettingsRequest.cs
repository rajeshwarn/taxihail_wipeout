#region

using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Route("/settings/payments/server", "GET")]
    [Authenticate(ApplyTo.All)]
    [AuthorizationRequired(ApplyTo.All, RoleName.SuperAdmin)]
    public class ServerPaymentSettingsRequest : IReturn<ServerPaymentSettingsResponse>
    {
    }
}