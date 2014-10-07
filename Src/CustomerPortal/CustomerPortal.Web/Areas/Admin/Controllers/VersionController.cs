#region

using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Domain;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Extensions;
using CustomerPortal.Web.Models;
using CustomerPortal.Web.Services;
using CustomerPortal.Web.Services.Impl;
using MongoRepository;
using Version = CustomerPortal.Web.Entities.Version;

#endregion

namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleName.Admin)]
    public class VersionController : CompanyControllerBase
    {
        private readonly IClock _clock;
        private readonly Func<string, string, IFileManager> _packageManagerFactory;

        public VersionController(IRepository<Company> repository, IClock clock,
            Func<string, string, IFileManager> packageManagerFactory)
            : base(repository)
        {
            _clock = clock;
            _packageManagerFactory = packageManagerFactory;
        }

        public VersionController()
            : this(new MongoRepository<Company>(), Clock.Instance, (id, version) => new PackageManager(id, version))
        {
        }

        //
        // GET: /Admin/Version/

        public ActionResult Index(string id)
        {
            var company = Repository.GetById(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            return
                View(
                    company.Versions.Select(x => VersionViewModel.CreateFrom(company, x))
                        .OrderByDescending(x => x.CreatedOn));
        }


        //
        // GET: /Admin/Version/Create

        public ActionResult Create(string id)
        {
            var company = Repository.GetById(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            ViewBag.Title = company.CompanyName;
            var model = new Version();
            model.WebsiteUrl = "https://services.taxihail.com/" + company.CompanyKey;

            return View(model);
        }

        //
        // POST: /Admin/Version/Create

        [HttpPost]
        public ActionResult Create(string id, Version model)
        {
            var company = Repository.GetById(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            ViewBag.Title = company.CompanyName;

            if (ModelState.IsValid)
            {
                try
                {
                    model.VersionId = Guid.NewGuid().ToString();
                    model.CreatedOn = _clock.UtcNow;
                    company.Versions.Add(model);
                    Repository.Update(company);


                    return RedirectToAction("CreateIpa", new {id, model.Number});
                }
                catch
                {
                    return View(model);
                }
            }
            return View(model);
        }

        public ActionResult CreateIpa(string id, string number)
        {
            var company = Repository.GetById(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            ViewBag.Title = company.CompanyName;
            var version = company.FindVersion(number);
            if (version == null)
            {
                return HttpNotFound();
            }
            return View(version);
        }

        [HttpPost]
        public ActionResult CreateIpa(string id, string number, HttpPostedFileBase file)
        {
            var company = Repository.GetById(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            ViewBag.Title = company.CompanyName;
            var version = company.FindVersion(number);
            if (version == null)
            {
                return HttpNotFound();
            }
            if (file != null)
            {
                version.IpaFilename = Path.GetFileName(file.FileName);
                var packages = _packageManagerFactory.Invoke(id, version.Number);
                packages.Save(file);

                Repository.Update(company);

                return Json(new
                {
                    name = version.IpaFilename,
                    size = file.ContentLength,
                    type = file.ContentType,
                    redirect = Url.Action("CreateApk", new {id, number})
                });
            }
            return Json(new Array[0]);
        }

        public ActionResult CreateApk(string id, string number)
        {
            var company = Repository.GetById(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            ViewBag.Title = company.CompanyName;
            var version = company.FindVersion(number);
            if (version == null)
            {
                return HttpNotFound();
            }
            return View(version);
        }

        [HttpPost]
        public ActionResult CreateApk(string id, string number, HttpPostedFileBase file)
        {
            var company = Repository.GetById(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            ViewBag.Title = company.CompanyName;
            var version = company.FindVersion(number);
            if (version == null)
            {
                return HttpNotFound();
            }
            if (file != null)
            {
                version.ApkFilename = Path.GetFileName(file.FileName);
                var packages = _packageManagerFactory.Invoke(id, version.Number);
                packages.Save(file);

                Repository.Update(company);

                return Json(new
                {
                    name = version.ApkFilename,
                    size = file.ContentLength,
                    type = file.ContentType,
                    redirect = Url.Action("Index", new {id})
                });
            }
            return Json(new Array[0]);
        }

        //
        // GET: /Admin/Version/Delete/5

        public ActionResult Delete(string id, string number)
        {
            var company = Repository.GetById(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            var version = company.FindVersion(number);
            if (version == null)
            {
                return HttpNotFound();
            }
            return View(VersionViewModel.CreateFrom(company, version));
        }

        //
        // POST: /Admin/Version/Delete/5

        [HttpPost]
        public ActionResult Delete(string id, string number, FormCollection collection)
        {
            var company = Repository.GetById(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            var version = company.FindVersion(number);
            if (version == null)
            {
                return HttpNotFound();
            }

            company.Versions.Remove(version);
            Repository.Update(company);

            var packages = _packageManagerFactory.Invoke(id, version.Number);
            packages.DeleteAll();

            return RedirectToAction("Index", new {id});
        }

        public ActionResult Detail(string compId, string id)
        {
            var company = new CompanyService(compId).GetCompany();
            var version = company.FindVersionById(id);
            if (version == null)
            {
                return HttpNotFound();
            }
            var viewModel = VersionViewModel.CreateFrom(company, version);
            return View(viewModel);
        }

        public ActionResult Email(string number, string companyId)
        {
            var company = new CompanyService(companyId).GetCompany();
            var version = company.FindVersion(number);
            if (version == null)
            {
                return HttpNotFound();
            }
            return View(new VersionEmailViewModel
            {
                Version = version,
                CompanyId = companyId
            });
        }

        [HttpPost]
        public ActionResult Email(string number, VersionEmailViewModel model)
        {
            var company = new CompanyService(model.CompanyId).GetCompany();
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