#region

using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class RulesServiceClient : BaseServiceClient
    {
        public RulesServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }

        public void CreateRule(Rule rule)
        {
            var req = string.Format("/admin/rules");
             Client.Post<string>(req, rule);
        }

        public void UpdateRule(Rule rule)
        {
            var req = string.Format("/admin/rules/" + rule.Id);
            Client.Put<string>(req, rule);
        }


        public void DeleteRule(Guid ruleId)
        {
            var req = string.Format("/admin/rules/" + ruleId);
            Client.Delete<string>(req);
        }

        public IList<Rule> GetRules()
        {
            var req = string.Format("/admin/rules");
            return Client.Get<IList<Rule>>(req);
        }

        public void ActivateRule(Guid ruleId)
        {
            var req = string.Format("/admin/rules/" + ruleId + "/activate");
            Client.Post<string>(req, new RuleActivateRequest {RuleId = ruleId});
        }

        public void DeactivateRule(Guid ruleId)
        {
            var req = string.Format("/admin/rules/" + ruleId + "/deactivate");
            Client.Post<string>(req, new RuleDeactivateRequest {RuleId = ruleId});
        }
    }
}