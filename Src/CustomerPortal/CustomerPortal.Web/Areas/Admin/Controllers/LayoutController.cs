#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Models;
using CustomerPortal.Web.Services;
using CustomerPortal.Web.Services.Impl;
using ExtendedMongoMembership;
using MongoRepository;

#endregion

namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleName.Admin)]
    public class LayoutController : Controller
    {
        private readonly MongoSession _session =
            new MongoSession(ConfigurationManager.ConnectionStrings["MongoServerSettings"].ConnectionString);

        public LayoutController(IRepository<Company> repository, Func<string, IFileManager> factory)
        {
            Repository = repository;
            LayoutManagerFactory = factory;
        }


        public LayoutController()
            : this(new MongoRepository<Company>(), id => new LayoutsManager(id))
        {
        }

        protected IRepository<Company> Repository { get; private set; }
        protected Func<string, IFileManager> LayoutManagerFactory { get; set; }

        public ActionResult Index(string id)
        {
            var company = Repository.GetById(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            ViewBag.CompanyName = company.CompanyName;
            var layouts = LayoutManagerFactory.Invoke(id);
            return View(new LayoutsViewModel
            {
                ApprovedDate = company.LayoutsApprovedOn.GetValueOrDefault(),
                IsApproved = company.LayoutsApprovedOn.HasValue,
                IsRejected = company.LayoutRejected,
                Layouts = layouts.GetAll(),
            });
        }

        [HttpPost]
        public ActionResult Index(string id, IEnumerable<HttpPostedFileBase> files)
        {
            bool changes = false;
            var layouts = LayoutManagerFactory.Invoke(id);
            foreach (var filesrc in files)
            {
                if (filesrc != null)
                {
                    foreach (var file in files.Where(x => x != null).Where(x => x.ContentLength > 0))
                    {
                        layouts.Save(file);
                    }

                    changes = true;
                }
            }
            if (changes)
            {
                var company = Repository.Single(c => c.Id == id);
                if (company == null)
                {
                    return HttpNotFound();
                }
                var oldStat = company.Status;
                var changeDate = DateTime.Now;
                company.Status = AppStatus.LayoutCompleted;
                company.LayoutRejected.Clear();
                Repository.Update(company);


                if (oldStat != company.Status)
                {
                    var users = _session.Users;
                    var emails = new List<string>();
                    foreach (var user in users)
                    {
                        foreach (var userrole in user.Roles)
                        {
                            if (userrole.RoleName == "admin")
                            {
                                emails.Add(user.UserName);
                            }
                        }
                    }

                    try
                    {
                        var isDisabled =
                            (bool) new AppSettingsReader().GetValue("DisableEmailNotification", typeof (bool));
                        if (isDisabled)
                        {
                            return RedirectToAction("Index", "Home");
                        }

                        var subject = String.Format("{0} status has changed to {1}.", company.CompanyName,
                            company.Status);
                        var body = String.Format("Status change for {0} on {1}. Status changed from {2} to {3}.",
                            company.CompanyName, changeDate, oldStat, company.Status);
                        foreach (var email in emails)
                        {
                            WebMail.Send(email, subject, body);
                        }
                        return RedirectToAction("Index", "Home");
                    }
                    catch (Exception e)
                    {
                        return RedirectToAction("Index", new {id});
                    }
                }
            }

            return RedirectToAction("Index", new {id});
        }

        [HttpPost]
        public ActionResult Delete(string id, string file)
        {
            var layouts = LayoutManagerFactory.Invoke(id);
            layouts.Delete(file);
            var company = Repository.Single(c => c.Id == id);
            var oldStat = company.Status;
            var changeDate = DateTime.Now;
            company.Status = AppStatus.Open;
            company.LayoutRejected.Clear();
            Repository.Update(company);
            ChangeStatusEmail.SendEmail(oldStat.ToString(), company.Status.ToString(), _session, changeDate,
                company.CompanyName, company.Payment.PONumber);
            return RedirectToAction("Index", new {id});
        }

        public ActionResult ResetApproval(string id)
        {
            var company = Repository.GetById(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            var oldStat = company.Status;
            var changeDate = DateTime.Now;
            company.LayoutsApprovedOn = null;
            company.Status = AppStatus.LayoutCompleted;
            Repository.Update(company);
            ChangeStatusEmail.SendEmail(oldStat.ToString(), company.Status.ToString(), _session, changeDate,
                company.CompanyName, company.Payment.PONumber);
            return RedirectToAction("Index", new {id});
        }


        public ActionResult Image(string id, string filename)
        {
            var layouts = LayoutManagerFactory.Invoke(id);
            string filepath;
            if (!layouts.Exists(filename, out filepath))
            {
                return HttpNotFound();
            }
            var mimeType = MimeMapping.GetMimeMapping(filepath);
            return base.File(filepath, mimeType);
        }
    }
}