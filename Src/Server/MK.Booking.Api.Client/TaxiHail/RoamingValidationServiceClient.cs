using System;
using System.Net.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Http.Extensions;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class RoamingValidationServiceClient
    {
        private readonly HttpClient _client;

        public string Url { get; private set; }

        public RoamingValidationServiceClient(string applicationKey, DeploymentTargets target)
        {
            Url = GetUrl(applicationKey, target);
            _client = new HttpClient { BaseAddress = new Uri(Url) };
        }

        public OrderValidationResult ValidateOrder(CreateOrderRequest orderRequest, bool forError = false)
        {
            try
            {
                var req = string.Format("api/account/orders/validate/" + forError);
                return _client.Post(req, orderRequest)
                        .Deserialize<OrderValidationResult>()
                        .Result;
            }
            catch(Exception)
            {
                // Error while contacting external rule server. Don't validate rules.
                return new OrderValidationResult { HasError = false };
            }
        }

        // WARNING: right now it's only used for zone validation, so to be compatible with test environments, we only set the target to be Production
        // This is ok right now since it's nothing important but if you need to call another taxihail server, make sure it's set correctly for your testing
        private string GetUrl(string applicationKey, DeploymentTargets target)
        {
            switch (target)
            {
                case DeploymentTargets.Local:
                case DeploymentTargets.Dev:
                    return string.Format("http://test.taxihail.biz:8181/{0}/", applicationKey);
                case DeploymentTargets.Staging:
                    return string.Format("https://staging.taxihail.com/{0}/", applicationKey);
                case DeploymentTargets.Production:
                    return string.Format("https://api.taxihail.com/{0}/", applicationKey);
                default:
                    return string.Format("http://test.taxihail.biz:8181/{0}/", applicationKey);
            }
        }
    }
}
