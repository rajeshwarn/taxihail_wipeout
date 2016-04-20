using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Account
{
    [Auth]
    public class PushNotificationRegistrationController : BaseApiController
    {
        public PushNotificationRegistrationService PushNotificationRegistrationService { get; private set; }

        public PushNotificationRegistrationController(PushNotificationRegistrationService pushNotificationRegistrationService)
        {
            PushNotificationRegistrationService = pushNotificationRegistrationService;
        }

        [HttpPost, Route("api/accounts/pushnotifications/{deviceToken}")]
        public IHttpActionResult RegisterForPushNotification(string deviceToken, [FromBody] PushNotificationRegistration request)
        {
            request.DeviceToken = deviceToken;

            PushNotificationRegistrationService.Post(request);

            return Ok();
        }

        [HttpDelete, Route("api/accounts/pushnotifications/{deviceToken}")]
        public IHttpActionResult DeletePushNotificationRegistration(string deviceToken)
        {
            PushNotificationRegistrationService.Delete(deviceToken);

            return Ok();
        }

    }
}
