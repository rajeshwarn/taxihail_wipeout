using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Microsoft.Practices.ServiceLocation;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Web.admin
{
    public partial class _default : apcurium.MK.Web.PageBase
    {
        protected string ApplicationKey { get; private set; }
        protected string ApplicationName { get; private set; }
        protected string ApplicationVersion { get; private set; }
        
        protected bool IsAuthenticated { get; private set; }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();

            ApplicationKey = config.GetSetting("TaxiHail.ApplicationKey");
            ApplicationName = config.GetSetting("TaxiHail.ApplicationName");

            ApplicationVersion = Assembly.GetAssembly(typeof (_default)).GetName().Version.ToString();
           
            IsAuthenticated = base.UserSession.IsAuthenticated;
            if(!base.UserSession.HasPermission(Permissions.Admin))
            {
                this.Response.Redirect("~");
            }
        }

    }
}