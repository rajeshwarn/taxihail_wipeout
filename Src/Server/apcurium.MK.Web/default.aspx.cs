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
        protected string JSAssetsSource { get; set; }
        protected string JSAppSource { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();

            ApplicationKey = config.GetSetting("TaxiHail.ApplicationKey");
            ApplicationName = config.GetSetting("TaxiHail.ApplicationName");
            DefaultLatitude = config.GetSetting("GeoLoc.DefaultLatitude");
            DefaultLongitude = config.GetSetting("GeoLoc.DefaultLongitude");
            DefaultPhoneNumber = config.GetSetting("DefaultPhoneNumberDisplay");
            IsAuthenticated = base.UserSession.IsAuthenticated;

#if DEBUG
            var reader = XDocument.Load(HostingEnvironment.MapPath("~/scripts/assets.xml")).Root.CreateReader();
            reader.MoveToContent();
            JSAssetsSource = reader.ReadInnerXml();

            reader = XDocument.Load(HostingEnvironment.MapPath("~/scripts/assets.xml")).Root.CreateReader();
            reader.MoveToContent();
            JSAppSource = reader.ReadInnerXml();
#else
            JSAssetsSource = "<script src='minified.assets.js'></script>";
            JSAppSource = "<script src='minified.app.js'></script>";
#endif
        }

    }
}