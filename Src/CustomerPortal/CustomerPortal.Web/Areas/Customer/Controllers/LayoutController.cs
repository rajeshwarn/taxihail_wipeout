#region

using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Models;
using CustomerPortal.Web.Security;
using CustomerPortal.Web.Services.Impl;
using ExtendedMongoMembership;
using MongoRepository;

#endregion

namespace CustomerPortal.Web.Areas.Customer.Controllers
{
    [Authorize(Roles = RoleName.Customer)]
    [CustomerCompanyFilter]
    public class LayoutController : CustomerControllerBase
    {
        private readonly MongoSession _session =
            new MongoSession(ConfigurationManager.ConnectionStrings["MongoServerSettings"].ConnectionString);

        protected internal LayoutsManager Layouts { get; set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var companyId = (string) filterContext.RouteData.Values["companyId"];
            Layouts = new LayoutsManager(companyId);
        }


        [ChildActionOnly]
        public ActionResult Index()
        {
            var company = Service.GetCompany();
            var viewModel = new LayoutsViewModel
            {
                CompanyId = company.Id,
                ApprovedDate = company.LayoutsApprovedOn.GetValueOrDefault(),
                IsApproved = company.LayoutsApprovedOn.HasValue,
                IsRejected = company.LayoutRejected,
                Layouts = Layouts.GetAll()
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Approve()
        {
            Service.ApproveLayouts();


            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Reject(LayoutRejectModel data)
        {
            var repository = new MongoRepository<Company>();

            foreach (var key in data.rejectLayout.Keys)
            {
                var date = DateTime.Now;
                var company = repository.GetById(key);
                var oldStat = company.Status;
                string reason = String.Empty;
                company.Status = AppStatus.LayoutRejected;
                company.LayoutsApprovedOn = null;
                company.LayoutRejected.Add(date, data.rejectLayout[key]);
                if (data.rejectLayout[key] != null)
                {
                    reason = "Reason: " + data.rejectLayout[key];
                }
                repository.Update(company);
                ChangeStatusEmail.SendEmail(oldStat.ToString(), company.Status.ToString(), _session, date,
                    company.CompanyName, reason);
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Image(string filename)
        {
            string filepath;
            if (!Layouts.Exists(filename, out filepath))
            {
                return HttpNotFound();
            }
            var mimeType = MimeMapping.GetMimeMapping(filepath);
            return base.File(filepath, mimeType);
        }
    }
}