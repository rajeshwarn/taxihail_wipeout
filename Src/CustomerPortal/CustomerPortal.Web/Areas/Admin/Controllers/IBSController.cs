#region

using System;
using System.Text;
using System.Web.Mvc;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.IBSServices;
using MongoRepository;

#endregion

namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleName.Admin)]
    public class IBSController : CompanyControllerBase
    {
        public IBSController(IRepository<Company> repository)
            : base(repository)
        {
        }

        public IBSController()
            : this(new MongoRepository<Company>())
        {
        }

        public ActionResult Index(string id)
        {
            var company = Repository.GetById(id);
            if (company == null) return HttpNotFound();

            ViewBag.CompanyName = company.CompanyName;
            ViewBag.Id = id;
            return View(company.IBS);
        }

        [HttpPost]
        public ActionResult Index(string id, IBSSettings model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var company = Repository.GetById(id);
                    if (company == null) return HttpNotFound();

                    company.IBS = model;
                    Repository.Update(company);

                    return RedirectToAction("Index", "Home");
                }
                catch
                {
                    return View(model);
                }
            }
            return View(model);
        }

        public ActionResult Test(string id, string serviceUrl, string userName, string password)
        {
            var script = new IBSTestScript();

            var result = new StringBuilder();
            var sucess = script.RunTest(serviceUrl, userName, password, ref result);


            var company = Repository.GetById(id);
            company.IBS.LastSucessfullTestDateTime = sucess ? DateTime.Now : (DateTime?) null;
            Repository.Update(company);


            return
                View(new IbsTestResult
                {
                    CompanyId = id,
                    Result = result.ToString(),
                    ServiceUrl = serviceUrl,
                    UserName = userName,
                    Password = password
                });
        }
    }
}