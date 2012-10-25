using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Microsoft.Practices.ServiceLocation;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Web
{
    public partial class _default : PageBase
    {
        protected string ApplicationKey { get; private set; }
        protected string ApplicationName { get; private set; }
        protected string DefaultLatitude { get; private set; }
        protected string DefaultLongitude { get; private set; }
        protected string DefaultPhoneNumber { get; private set; }
        protected bool IsAuthenticated { get; private set; }
        protected string FacebookAppId { get; private set; }
        protected string FacebookEnabled { get; private set; }
        protected string HideDispatchButton { get; private set; }
        protected string GeolocPopularRange { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();

            ApplicationKey = config.GetSetting("TaxiHail.ApplicationKey");
            ApplicationName = config.GetSetting("TaxiHail.ApplicationName");
            DefaultLatitude = config.GetSetting("GeoLoc.DefaultLatitude");
            DefaultLongitude = config.GetSetting("GeoLoc.DefaultLongitude");
            DefaultPhoneNumber = config.GetSetting("DefaultPhoneNumberDisplay");
            IsAuthenticated = base.UserSession.IsAuthenticated;
            FacebookAppId = config.GetSetting("FacebookAppId");
            FacebookEnabled = config.GetSetting("FacebookEnabled");
            HideDispatchButton = config.GetSetting("Client.HideCallDispatchButton");
            GeolocPopularRange = config.GetSetting("Geoloc.PopularAddress.Range");
        }

    }
}