#region

using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CustomerPortal.Web.Security;
using CustomerPortal.Web.Services;
using CustomerPortal.Web.Services.Impl;

#endregion

namespace CustomerPortal.Web.Areas.Customer.Controllers
{
    [Authorize(Roles = RoleName.Customer)]
    [CustomerCompanyFilter]
    public class GraphicsController : CustomerControllerBase
    {
        protected internal IFileManager Graphics { get; set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var companyId = (string) filterContext.RouteData.Values["companyId"];
            Graphics = new GraphicsManager(companyId);
        }

        [ChildActionOnly]
        public ActionResult Index()
        {
            return View(Graphics.GetAll());
        }

        [HttpPost]
        public ActionResult Upload(IEnumerable<HttpPostedFileBase> graphics)
        {
            if (graphics != null)
            {
                foreach (var graphic in graphics.Where(x => x != null).Where(x => x.ContentLength > 0))
                {
                    Graphics.Save(graphic);
                }
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Image(string filename)
        {
            string filepath;
            if (!Graphics.Exists(filename, out filepath))
            {
                return HttpNotFound();
            }
            var mimeType = MimeMapping.GetMimeMapping(filepath);
            return base.File(filepath, mimeType);
        }

        [HttpPost]
        public ActionResult Delete(string file)
        {
            Graphics.Delete(file);

            return RedirectToAction("Index", "Home");
        }
    }
}