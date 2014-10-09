#region

using System.Web.Mvc;
using CustomerPortal.Web.Domain;

#endregion

namespace CustomerPortal.Web.Areas.Customer.Controllers
{
    public class CustomerControllerBase : Controller
    {
        /// <summary>
        /// This property is internal so it can be injected for unit testing
        /// </summary>
        protected internal ICompanyService Service { get; set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // companyId was set by CustomerCompanyFilter
            var companyId = (string) filterContext.RouteData.Values["companyId"];
            Service = new CompanyService(companyId);
        }
    }
}