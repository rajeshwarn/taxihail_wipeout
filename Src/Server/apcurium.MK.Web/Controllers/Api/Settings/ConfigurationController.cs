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
    [RoutePrefix("settings")]
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

        [HttpGet, Route("reset")]
        public IHttpActionResult ResetConfiguration()
        {
            _configurationResetService.Get();

            return GenerateActionResult(true);
        }

        [HttpGet]
        public IHttpActionResult GetAppSettings()
        {
            var result = _configurationsService.Get(new ConfigurationsRequest());
            
            return GenerateActionResult(result);
        }

        [HttpGet, Route("encrypted")]
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

        [HttpGet, Auth, Route("notifications/{accountId:Guid?}")]
        public IHttpActionResult GetNotificationSettings(Guid? accountId)
        {
            var result = _configurationsService.Get(new NotificationSettingsRequest() {AccountId = accountId});

            return GenerateActionResult(result);
        }

        [HttpPost, Auth, Route("notifications/{accountId:Guid?}")]
        public IHttpActionResult UpdateNotificationSettings(Guid? accountId, NotificationSettingsRequest request)
        {
            request.AccountId = accountId;

            _configurationsService.Post(request);

            return Ok();
        }

        [HttpGet, Auth, Route("taxihailnetwork/{accountId:Guid?}")]
        public IHttpActionResult GetUserTaxiHailNetworkSettings(Guid? accountId, UserTaxiHailNetworkSettingsRequest request)
        {
            request.AccountId = accountId;

            var result = _configurationsService.Get(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Auth, Route("taxihailnetwork/{accountId:Guid?}")]
        public IHttpActionResult UpdateUserTaxiHailNetworkSettings(Guid? accountId, UserTaxiHailNetworkSettingsRequest request)
        {
            request.AccountId = accountId;

            _configurationsService.Post(request);

            return Ok();
        }
    }
}
