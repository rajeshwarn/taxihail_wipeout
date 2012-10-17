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

namespace apcurium.MK.Web.admin
{
    public partial class _default : apcurium.MK.Web.PageBase
    {
        protected string ApplicationKey { get; private set; }
        protected string ApplicationName { get; private set; }
        
        protected bool IsAuthenticated { get; private set; }
        
        protected string JSAssetsSource { get; set; }
        protected string JSAppSource { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();

            ApplicationKey = config.GetSetting("TaxiHail.ApplicationKey");
            ApplicationName = config.GetSetting("TaxiHail.ApplicationName");
           
            IsAuthenticated = base.UserSession.IsAuthenticated;
           

#if DEBUG
            var reader = XDocument.Load(HostingEnvironment.MapPath("~/admin/scripts/assets.xml")).Root.CreateReader();
            reader.MoveToContent();
            JSAssetsSource = reader.ReadInnerXml();

            reader = XDocument.Load(HostingEnvironment.MapPath("~/admin/scripts/app.xml")).Root.CreateReader();
            reader.MoveToContent();
            JSAppSource = reader.ReadInnerXml();
#else
            JSAssetsSource = "<script src='scripts/minified.assets.js'></script>";
            JSAppSource = "<script src='scripts/minified.app.js'></script>";
#endif
        }

    }
}