using System;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class RulesServiceClient : BaseServiceClient
    {
        public RulesServiceClient(string url, string sessionId, string userAgent)
            : base(url, sessionId, userAgent)
        {
        }

        public void CreateRule(Rule rule)
        {
            var req = string.Format("/admin/rules");
            var response = Client.Post<string>(req, rule);
        }

        public void UpdateRule(Rule rule)
        {
            var req = string.Format("/admin/rules/" + rule.Id);
            var response = Client.Put<string>(req,rule);
        }


        public void DeleteRule(Guid ruleId)
        {
            var req = string.Format("/admin/rules/" + ruleId);
            var response = Client.Delete<string>(req);
        }

        public IList<Rule> GetRules()
        {
            var req = string.Format("/admin/rules");
            return Client.Get<IList<Rule>>(req);
        }
        
        public void ActivateRule(Guid ruleId)
        {
            var req = string.Format("/admin/rules/" + ruleId + "/activate" );
            Client.Post<string>(req, new RuleActivateRequest { RuleId = ruleId });
        }
        
        public void DeactivateRule(Guid ruleId)
        {
            var req = string.Format("/admin/rules/" + ruleId + "/deactivate" );
            Client.Post<string>(req, new RuleDeactivateRequest { RuleId = ruleId });
        }
    }
}