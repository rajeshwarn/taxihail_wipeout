#region

using System.Web.Http;
using apcurium.MK.Web;
using apcurium.MK.Web.App_Start;
using Funq;
using log4net;
using Microsoft.Owin;
using Owin;

#endregion

[assembly: OwinStartup(typeof(MkWebAppHost))]
namespace apcurium.MK.Web
{
    public partial class MkWebAppHost 
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (MkWebAppHost));

        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}