﻿using System;
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

        public OrderValidationResult ValidateOrder(CreateOrder order, bool forError = false)
        {
            var req = string.Format("api/account/orders/validate/" + forError);
            return _client.Post(req, order)
                    .Deserialize<OrderValidationResult>()
                    .Result;
        }

        private string GetUrl(string applicationKey, DeploymentTargets target)
        {
            switch (target)
            {
                case DeploymentTargets.Local:
                    return string.Format("http://test.taxihail.biz:8181/{0}/", applicationKey);
                case DeploymentTargets.Staging:
                    return string.Format("http://test.taxihail.biz:8181/{0}/", applicationKey);
                case DeploymentTargets.Production:
                    return string.Format("http://services.taxihail.com/{0}/", applicationKey);
                default:
                    return string.Format("http://test.taxihail.biz:8181/{0}/", applicationKey);
            }
        }
    }
}
