using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    [Auth(Role = RoleName.Admin)]
    public class NotificationAdministrationController : BaseApiController
    {
        public NotificationAdministrationService NotificationAdministrationService { get; private set; }

        public NotificationAdministrationController(NotificationAdministrationService notificationAdministrationService)
        {
            NotificationAdministrationService = notificationAdministrationService;
        }

        [HttpPost, Route("api/v2/admin/testemail/{emailAddress}")]
        public async Task<IHttpActionResult> TestEmail(string emailAddress, [FromBody] TestEmailAdministrationRequest request)
        {
            request.EmailAddress = emailAddress;

            await NotificationAdministrationService.Post(request);

            return Ok();
        }

        [HttpPost, Route("api/v2/admin/pushnotifications/{emailAddress}")]
        public IHttpActionResult TestPushNotification(string emailAddress, [FromBody] PushNotificationAdministrationRequest request)
        {
            request.EmailAddress = emailAddress;

            NotificationAdministrationService.Post(request);

            return Ok();
        }

        [HttpGet, Route("api/v2/admin/testemail/templates")]
        public IHttpActionResult GetEmailTemplateNames()
        {
            var result = NotificationAdministrationService.Get(new EmailTemplateNamesRequest());

            return GenerateActionResult(result);
        }
    }
}
