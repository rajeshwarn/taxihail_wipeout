using System;
using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Settings
{
    public class ConfigurationController : BaseApiController
    {
        private readonly ConfigurationResetService _configurationResetService;
        private readonly ConfigurationsService _configurationsService;

        public ConfigurationController(ICacheClient cacheClient, IServerSettings serverSettings, ICommandBus commandBus, IConfigurationDao configDao)
        {
            _configurationResetService = new ConfigurationResetService(cacheClient, serverSettings);

            _configurationsService = new ConfigurationsService(serverSettings, commandBus, configDao);
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            PrepareApiServices(_configurationsService, _configurationResetService);
        }

        [HttpGet, Route("api/v2/settings/reset")]
        public IHttpActionResult ResetConfiguration()
        {
            _configurationResetService.Get();

            return GenerateActionResult(true);
        }

        [HttpGet, Route("api/v2/settings")]
        public IHttpActionResult GetAppSettings()
        {
            var result = _configurationsService.Get(new ConfigurationsRequest());
            
            return GenerateActionResult(result);
        }

        [HttpGet, Route("api/v2/settings/encrypted")]
        public IHttpActionResult GetEncryptedSettings()
        {
            var result = _configurationsService.Get(new EncryptedConfigurationsRequest());

            return GenerateActionResult(result);
        }

        [HttpPost, Auth(Role = RoleName.Admin), Route("settings")]
        public IHttpActionResult UpdateSettings(ConfigurationsRequest request)
        {
            _configurationsService.Post(request);

            return Ok();
        }

        [HttpGet, Auth, Route("api/v2/settings/notifications")]
        public IHttpActionResult GetNotificationSettings()
        {
            return GetNotificationSettings(null);
        }

        [HttpGet, Auth, Route("api/v2/settings/notifications/{accountId}")]
        public IHttpActionResult GetNotificationSettings(Guid? accountId)
        {
            var result = _configurationsService.Get(new NotificationSettingsRequest() {AccountId = accountId});

            return GenerateActionResult(result);
        }

        [HttpPost, Auth, Route("api/v2/settings/notifications")]
        public IHttpActionResult UpdateNotificationSettings([FromBody] NotificationSettingsRequest request)
        {
            return UpdateNotificationSettings(null, request);
        }

        [HttpPost, Auth, Route("api/v2/settings/notifications/{accountId}")]
        public IHttpActionResult UpdateNotificationSettings(Guid? accountId, [FromBody]NotificationSettingsRequest request)
        {
            request.AccountId = accountId;

            _configurationsService.Post(request);

            return Ok();
        }

        [HttpGet, Auth, Route("api/v2/settings/taxihailnetwork")]
        public IHttpActionResult GetUserTaxiHailNetworkSettings(UserTaxiHailNetworkSettingsRequest request)
        {
            return GetUserTaxiHailNetworkSettings(null, request);
        }

        [HttpGet, Auth, Route("api/v2/settings/taxihailnetwork/{accountId}")]
        public IHttpActionResult GetUserTaxiHailNetworkSettings(Guid? accountId, UserTaxiHailNetworkSettingsRequest request)
        {
            request.AccountId = accountId;

            var result = _configurationsService.Get(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Auth, Route("api/v2/settings/taxihailnetwork")]
        public IHttpActionResult UpdateUserTaxiHailNetworkSettings(UserTaxiHailNetworkSettingsRequest request)
        {
            return UpdateUserTaxiHailNetworkSettings(null, request);
        }

        [HttpPost, Auth, Route("api/v2/settings/taxihailnetwork/{accountId}")]
        public IHttpActionResult UpdateUserTaxiHailNetworkSettings(Guid? accountId, UserTaxiHailNetworkSettingsRequest request)
        {
            request.AccountId = accountId;

            _configurationsService.Post(request);

            return Ok();
        }
    }
}
