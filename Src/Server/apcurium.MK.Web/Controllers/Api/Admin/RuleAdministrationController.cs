using System;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    [RoutePrefix("admin/rules")]
    public class RuleAdministrationController : BaseApiController
    {
        public RuleActivateService RuleActivateService { get; }
        public RuleDeactivateService RuleDeactivateService { get; }
        public RulesService RulesService { get; }

        public RuleAdministrationController(IRuleDao ruleDao, ICommandBus commandBus)
        {
            RuleActivateService = new RuleActivateService(commandBus);
            RuleDeactivateService = new RuleDeactivateService(commandBus);
            RulesService = new RulesService(ruleDao, commandBus);
        }

        [HttpPost, Route("{ruleId}/activate"), Auth(Role = RoleName.Admin)]
        public IHttpActionResult ActivateRule(Guid ruleId)
        {
            var result = RuleActivateService.Post(new RuleActivateRequest() {RuleId = ruleId});

            return GenerateActionResult(result);
        }

        [HttpPost, Route("{ruleId}/deactivate"), Auth(Role = RoleName.Admin)]
        public IHttpActionResult DeactivateRule(Guid ruleId)
        {
            var result = RuleDeactivateService.Post(new RuleDeactivateRequest() {RuleId = ruleId});

            return GenerateActionResult(result);
        }
        [HttpGet, Auth]
        public IHttpActionResult GetRules()
        {
            var result = RulesService.Get();

            return GenerateActionResult(result);
        }

        [HttpPost, Auth(Role = RoleName.Admin)]
        public IHttpActionResult CreateRule(RuleRequest request)
        {
            var result = RulesService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPut, Auth(Role = RoleName.Admin), Route("{id}")]
        public IHttpActionResult UpdateRule(Guid ruleId, RuleRequest request)
        {
            request.Id = ruleId;

            RulesService.Put(request);

            return Ok();
        }

        [HttpDelete, Auth(Role = RoleName.Admin), Route("{id}")]
        public IHttpActionResult DeleteRule(Guid ruleId)
        {
            RulesService.Delete(ruleId);

            return Ok();
        }

    }
}
