#region

using System.Web.Mvc;

#endregion

namespace CustomerPortal.Web.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "Admin"; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Admin_files",
                "Admin/Layout/{id}/{filename}/{extension}",
                new {controller = "Layout", action = "Index"}
                );

            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new {controller = "Home", action = "Index", id = UrlParameter.Optional}
                );
        }
    }
}