using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    [RoutePrefix("admin"), Auth(Role = RoleName.Admin)]
    public class NotificationAdministrationController : BaseApiController
    {
        public NotificationAdministrationService NotificationAdministrationService { get; }

        public NotificationAdministrationController(IAccountDao accountDao, IDeviceDao deviceDao, INotificationService notificationService, IServerSettings serverSettings)
        {
            NotificationAdministrationService = new NotificationAdministrationService(accountDao, deviceDao, notificationService, serverSettings, Logger);
        }

        [HttpPost, Route("testemail/{emailAddress}")]
        public IHttpActionResult TestEmail(string emailAddress, [FromBody] TestEmailAdministrationRequest request)
        {
            request.EmailAddress = emailAddress;

            NotificationAdministrationService.Post(request);

            return Ok();
        }

        [HttpPost, Route("pushnotifications/{emailAddress}")]
        public IHttpActionResult TestPushNotification(string emailAddress, [FromBody] PushNotificationAdministrationRequest request)
        {
            request.EmailAddress = emailAddress;

            NotificationAdministrationService.Post(request);

            return Ok();
        }

        [HttpGet, Route("testmail/templates")]
        public IHttpActionResult GetEmailTemplateNames()
        {
            var result = NotificationAdministrationService.Get(new EmailTemplateNamesRequest());

            return GenerateActionResult(result);
        }
    }
}
