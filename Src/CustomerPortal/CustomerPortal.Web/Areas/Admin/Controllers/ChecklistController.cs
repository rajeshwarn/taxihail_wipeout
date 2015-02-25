using System.Linq;
using System.Web.Mvc;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Entities;
using MongoRepository;
using System.Net;
using System.IO;
using System;
using CustomerPortal.Web.Services.Impl;


namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    public class ChecklistController : Controller
    {
        private readonly MongoRepository<Company> repository;

        public ChecklistController()
        {
            repository = new MongoRepository<Company>();
        }

        //
        // GET: /Admin/Apple/

        public ActionResult Index()
        {
            var companyList = repository.Where(x => x.Status != AppStatus.DemoSystem).OrderBy(x => x.CompanyName).Select(c => new ChecklistModel 
            { 
                Company = c,
                AppStoreCred = (!string.IsNullOrEmpty(c.AppleAppStoreCredentials.Username) && !string.IsNullOrEmpty(c.AppleAppStoreCredentials.Password)).ToString(),
                PlayStoreCred = (!string.IsNullOrEmpty(c.GooglePlayCredentials.Username) && !string.IsNullOrEmpty(c.GooglePlayCredentials.Password)).ToString(),
                IBS = c.IsValid().ToString(),
                UDID = (c.Store.UniqueDeviceIdentificationNumber.Count() > 0).ToString(),
                PONumber = (c.Payment.PONumber == null ? "Null": c.Payment.PONumber),
                Status = c.Status.ToString()
            }).ToArray();

            
            return View(companyList);

        }
    }
}
