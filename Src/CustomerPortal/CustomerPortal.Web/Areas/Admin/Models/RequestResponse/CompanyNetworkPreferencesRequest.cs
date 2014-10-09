using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using CustomerPortal.Web.Entities.Network;

namespace CustomerPortal.Web.Areas.Admin.Models.RequestResponse
{
    [Route("api/customer/{companyId}/network/")]
    public class CompanyNetworkPreferencesRequest
    {
        public string CompanyId { get; set; }

        public CompanyPreference[] Preferences { get; set; }
    }
}