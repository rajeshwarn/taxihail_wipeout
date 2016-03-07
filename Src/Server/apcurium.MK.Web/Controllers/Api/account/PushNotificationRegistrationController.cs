using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Account
{
    [RoutePrefix("api/v2/account/pushnotifications"), Auth]
    public class PushNotificationRegistrationController : BaseApiController
    {
        public PushNotificationRegistrationService PushNotificationRegistrationService { get; }

        public PushNotificationRegistrationController(IAccountDao accountDao, IDeviceDao deviceDao)
        {
            PushNotificationRegistrationService = new PushNotificationRegistrationService(accountDao, deviceDao);
        }

        [HttpPost, Route("{deviceToken}")]
        public IHttpActionResult RegisterForPushNotification(string deviceToken, [FromBody] PushNotificationRegistration request)
        {
            request.DeviceToken = deviceToken;

            PushNotificationRegistrationService.Post(request);

            return Ok();
        }

        [HttpPost, Route("{deviceToken}")]
        public IHttpActionResult DeletePushNotificationRegistration(string deviceToken)
        {
            PushNotificationRegistrationService.Delete(deviceToken);

            return Ok();
        }

    }
}
