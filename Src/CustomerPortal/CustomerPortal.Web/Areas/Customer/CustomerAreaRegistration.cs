#region

using System.Web.Mvc;

#endregion

namespace CustomerPortal.Web.Areas.Customer
{
    public class CustomerAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "Customer"; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Customer_files",
                "Customer/Graphics/{filename}/{extension}",
                new {controller = "Graphics", action = "Index"}
                );

            context.MapRoute(
                "Customer_layouts",
                "Admin/Layout/{filename}/{extension}",
                new {controller = "Layout", action = "Index"}
                );

            context.MapRoute(
                "Customer_default",
                "Customer/{controller}/{action}/{id}",
                new {action = "Index", id = UrlParameter.Optional}
                );
        }
    }
}