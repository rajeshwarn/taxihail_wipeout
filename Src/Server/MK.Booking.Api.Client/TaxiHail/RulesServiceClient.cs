#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class RulesServiceClient : BaseServiceClient
    {
        public RulesServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService)
            : base(url, sessionId, packageInfo, connectivityService, null)
        {
        }

        public Task CreateRule(Rule rule)
        {
             return Client.PostAsync<object>("/admin/rules", rule);
        }

        public Task UpdateRule(Rule rule)
        {
            var req = string.Format("/admin/rules/" + rule.Id);
            return Client.PutAsync<object>(req, rule);
        }


        public Task DeleteRule(Guid ruleId)
        {
            var req = string.Format("/admin/rules/" + ruleId);
            return Client.DeleteAsync<object>(req);
        }

        public Task<IList<Rule>> GetRules()
        {
            return Client.GetAsync<IList<Rule>>("/admin/rules");
        }

        public Task ActivateRule(Guid ruleId)
        {
            var req = string.Format("/admin/rules/" + ruleId + "/activate");
            return Client.PostAsync<object>(req, new RuleActivateRequest {RuleId = ruleId});
        }

        public Task DeactivateRule(Guid ruleId)
        {
            var req = string.Format("/admin/rules/" + ruleId + "/deactivate");
            return Client.PostAsync<object>(req, new RuleDeactivateRequest {RuleId = ruleId});
        }
    }
}