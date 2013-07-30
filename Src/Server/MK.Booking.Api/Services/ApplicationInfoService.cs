using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Services
{
    public class ApplicationInfoService : RestServiceBase<ApplicationInfoRequest>
    {
        
        
        private readonly IConfigurationManager _configManager;
        public ApplicationInfoService(IConfigurationManager configManager)
        {            
            _configManager = configManager;
        }


        public override object OnGet(ApplicationInfoRequest request)
        {
            var info = new ApplicationInfo
                {
                    Version = Assembly.GetAssembly(typeof(ApplicationInfoService)).GetName().Version.ToString(),
                    SiteName =  _configManager.GetSetting("TaxiHail.SiteName")
                };
            return info;
        }


    }
}
