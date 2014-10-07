#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;
using Cupertino;
using CustomerPortal.Web.Entities;
using ExtendedMongoMembership;
using MongoRepository;
using Newtonsoft.Json;
using ApplicationInfo = CustomerPortal.Web.Areas.Admin.Models.ApplicationInfo;

#endregion

namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleName.Admin)]
    public class PaymentController : CompanyControllerBase
    {
        private readonly MongoSession _session =
            new MongoSession(ConfigurationManager.ConnectionStrings["MongoServerSettings"].ConnectionString);

        public PaymentController(IRepository<Company> repository)
            : base(repository)
        {
        }

        public PaymentController()
            : this(new MongoRepository<Company>())
        {
        }



        //
        // GET: /Customer/Home/

        public ActionResult Index(bool noCharge = false)
        {
            var repository = new MongoRepository<Company>();

            var companies  = repository.OrderBy(x => x.CompanyName).ToArray().Where(c=>c.Payment.NoCharge == noCharge);
            return View(companies);
        }

        //
        // GET: /Admin/Company/Create



        //
        // GET: /Admin/Company/Edit/5

        public ActionResult Edit(string id)
        {
            var company = Repository.GetById(id);
            if (company == null) return HttpNotFound();

            return View(company);
        }

        //
        // POST: /Admin/Company/Edit/5

        [HttpPost]
        public ActionResult Edit(string id, Company model)
        {
            try
            {
                var company = Repository.GetById(id);
                if (company == null) return HttpNotFound();


                company.Payment.PODate = model.Payment.PODate;
                company.Payment.PONumber = model.Payment.PONumber;
                company.Payment.QtyLicense = model.Payment.QtyLicense;
                company.Payment.NoCharge = model.Payment.NoCharge;

                company.Payment.TestLinkDate = model.Payment.TestLinkDate;
                company.Payment.TestLinkInvoiceNumber = model.Payment.TestLinkInvoiceNumber;

                company.Payment.PublishToStoreDate = model.Payment.PublishToStoreDate;
                company.Payment.PublishToStoreInvoiceNumber = model.Payment.PublishToStoreInvoiceNumber;
                
                Repository.Update(company);
                return RedirectToAction("Index");

                
            }
            catch
            {
                return View(model);
            }
        }



    }
}