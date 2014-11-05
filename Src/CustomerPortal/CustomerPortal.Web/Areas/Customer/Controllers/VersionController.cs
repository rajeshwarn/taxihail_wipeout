#region

using System;
using System.Web.Helpers;
using System.Web.Mvc;
using CustomerPortal.Web.Areas.Customer.Models;
using CustomerPortal.Web.Extensions;
using CustomerPortal.Web.Models;
using CustomerPortal.Web.Security;

#endregion

namespace CustomerPortal.Web.Areas.Customer.Controllers
{
    [Authorize(Roles = RoleName.Customer)]
    [CustomerCompanyFilter]
    public class VersionController : CustomerControllerBase
    {
        [ChildActionOnly]
        public ActionResult Index()
        {
            var company = Service.GetCompany();
            return View(company.Versions);
        }

        public ActionResult Email(string number)
        {
            var company = Service.GetCompany();
            var version = company.FindVersion(number);
            if (version == null)
            {
                return HttpNotFound();
            }
            return View(new VersionEmailViewModel
            {
                Version = version,
            });
        }

        [HttpPost]
        public ActionResult Email(string number, VersionEmailViewModel model)
        {
            var company = Service.GetCompany();
            var version = company.FindVersion(number);
            if (version == null)
            {
                return HttpNotFound();
            }
            model.Version = version;

            if (ModelState.IsValid)
            {
                try
                {
                    var appName = company.Application.AppName ?? company.CompanyName;
                    var subject = appName + " version " + version.Number;
                    var body = this.RenderPartialViewToString("_Email", VersionViewModel.CreateFrom(company, version));
                    WebMail.Send(model.RecipientEmailAddress, subject, body);

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception e)
                {
                    return View(model);
                }
            }
            return View(model);
        }
    }
}