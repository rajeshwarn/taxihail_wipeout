using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Extensions;
using ServiceStack.ServiceInterface;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Api.Services
{
    public class AppSettingsService : RestServiceBase<AppSettingsRequest>
    {
        private IConfigurationManager _confiManager;
        public AppSettingsService(IConfigurationManager confiManager)
        {
            _confiManager = confiManager;
        }

        public override object OnGet(AppSettingsRequest request)
        {
            if (request.SettingKey.HasValue())
            {
                return _confiManager.GetSetting(request.SettingKey);
            }
            else
            {
                return null; ;
            }
        }

    }
}
