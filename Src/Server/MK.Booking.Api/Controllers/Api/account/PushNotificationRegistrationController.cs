using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Account
{
    [RoutePrefix("api/v2/accounts/pushnotifications"), Auth]
    public class PushNotificationRegistrationController : BaseApiController
    {
        public PushNotificationRegistrationService PushNotificationRegistrationService { get; private set; }

        public PushNotificationRegistrationController(PushNotificationRegistrationService pushNotificationRegistrationService)
        {
            PushNotificationRegistrationService = pushNotificationRegistrationService;
        }

        [HttpPost, Route("{deviceToken}")]
        public IHttpActionResult RegisterForPushNotification(string deviceToken, [FromBody] PushNotificationRegistration request)
        {
            request.DeviceToken = deviceToken;

            PushNotificationRegistrationService.Post(request);

            return Ok();
        }

        [HttpDelete, Route("{deviceToken}")]
        public IHttpActionResult DeletePushNotificationRegistration(string deviceToken)
        {
            PushNotificationRegistrationService.Delete(deviceToken);

            return Ok();
        }

    }
}
