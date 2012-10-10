using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Practices.ServiceLocation;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Web
{
    public partial class _default : PageBase
    {
        protected string ApplicationKey { get; private set; }
        protected string DefaultLatitude { get; private set; }
        protected string DefaultLongitude { get; private set; }
        protected bool IsAuthenticated { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();

            ApplicationKey = config.GetSetting("TaxiHail.ApplicationKey");
            DefaultLatitude = config.GetSetting("GeoLoc.DefaultLatitude");
            DefaultLongitude = config.GetSetting("GeoLoc.DefaultLongitude");
            IsAuthenticated = base.UserSession.IsAuthenticated;
        }
    }
}