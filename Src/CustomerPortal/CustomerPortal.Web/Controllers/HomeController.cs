#region

using System.Web.Mvc;
using WebMatrix.WebData;

#endregion

namespace CustomerPortal.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (base.User.Identity.IsAuthenticated)
            {
                if (User.IsInRole(RoleName.Admin))
                {
                    return RedirectToAction("Index", "Home", new {area = "admin"});
                }
                if (User.IsInRole(RoleName.Customer))
                {
                    return RedirectToAction("Index", "Home", new {area = "customer"});
                }
                // User is authenticated, but does not have required role
                // Signout out user before redirecting him to login page
                WebSecurity.Logout();
            }
            return RedirectToAction("Login", "Account");
        }
    }
}