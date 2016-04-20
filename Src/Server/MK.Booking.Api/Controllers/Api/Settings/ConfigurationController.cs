using System;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Settings
{
    [NoCache]
    public class ConfigurationController : BaseApiController
    {
        public ConfigurationResetService ResetService { get; private set; }
        public ConfigurationsService Service { get; private set; }

        public ConfigurationController(ConfigurationResetService resetService, ConfigurationsService service)
        {
            ResetService = resetService;
            Service = service;
        }

        [HttpGet, Route("api/settings/reset")]
        public IHttpActionResult ResetConfiguration()
        {
            ResetService.Get();

            return GenerateActionResult(true);
        }

        [HttpGet, Route("api/settings")]
        public IHttpActionResult GetAppSettings()
        {
            var result = Service.Get(new ConfigurationsRequest());
            
            return GenerateActionResult(result, useCameCase: false);
        }

        [HttpGet, Route("api/settings/encrypted")]
        public IHttpActionResult GetEncryptedSettings()
        {
            var result = Service.Get(new EncryptedConfigurationsRequest());

            return GenerateActionResult(result, useCameCase: false);
        }

        [HttpPost, Auth(Role = RoleName.Admin), Route("settings")]
        public IHttpActionResult UpdateSettings(ConfigurationsRequest request)
        {
            Service.Post(request);

            return Ok();
        }

        [HttpGet, Auth, Route("api/settings/notifications")]
        public IHttpActionResult GetNotificationSettings()
        {
            return GetNotificationSettings(null);
        }

        [HttpGet, Auth, Route("api/settings/notifications/{accountId}")]
        public IHttpActionResult GetNotificationSettings(Guid? accountId)
        {
            var result = Service.Get(new NotificationSettingsRequest() {AccountId = accountId});

            return GenerateActionResult(result);
        }

        [HttpPost, Auth, Route("api/settings/notifications")]
        public IHttpActionResult UpdateNotificationSettings([FromBody] NotificationSettingsRequest request)
        {
            return UpdateNotificationSettings(null, request);
        }

        [HttpPost, Auth, Route("api/settings/notifications/{accountId}")]
        public IHttpActionResult UpdateNotificationSettings(Guid? accountId, [FromBody]NotificationSettingsRequest request)
        {
            request.AccountId = accountId;

            Service.Post(request);

            return Ok();
        }

        [HttpGet, Auth, Route("api/settings/taxihailnetwork")]
        public IHttpActionResult GetUserTaxiHailNetworkSettings()
        {
            return GetUserTaxiHailNetworkSettings(null);
        }

        [HttpGet, Auth, Route("api/settings/taxihailnetwork/{accountId}")]
        public IHttpActionResult GetUserTaxiHailNetworkSettings(Guid? accountId)
        {
            var result = Service.Get(new UserTaxiHailNetworkSettingsRequest()
            {
                AccountId = accountId
            });

            return GenerateActionResult(result);
        }

        [HttpPost, Auth, Route("api/settings/taxihailnetwork")]
        public IHttpActionResult UpdateUserTaxiHailNetworkSettings(UserTaxiHailNetworkSettingsRequest request)
        {
            return UpdateUserTaxiHailNetworkSettings(null, request);
        }

        [HttpPost, Auth, Route("api/settings/taxihailnetwork/{accountId}")]
        public IHttpActionResult UpdateUserTaxiHailNetworkSettings(Guid? accountId, UserTaxiHailNetworkSettingsRequest request)
        {
            request.AccountId = accountId;

            Service.Post(request);

            return Ok();
        }
    }
}
