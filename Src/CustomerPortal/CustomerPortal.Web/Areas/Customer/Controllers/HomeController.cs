#region
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Models;
using CustomerPortal.Web.Security;
using MongoRepository;
using System;


#endregion

namespace CustomerPortal.Web.Areas.Customer.Controllers
{
    [Authorize(Roles = RoleName.Customer)]
    [CustomerCompanyFilter]
    public class HomeController : CustomerControllerBase
    {
        //
        // GET: /Customer/Home/

        public ActionResult Index()
        {
            var company = Service.GetCompany();
            
            AddToAccessHistory(company.Id);

            return View(CompanyViewModel.CreateFrom(company));
        }
        
        
        private void AddToAccessHistory(string companyId)
        {
            var repository = new MongoRepository<UserPreference>();
            var user = repository.SingleOrDefault(u => u.UserIdentity == User.Identity.Name);
            if (user == null)
            {
                user = new UserPreference();
                user.Id = Guid.NewGuid().ToString();
                user.UserIdentity = User.Identity.Name;
            }

            if ( user.LastAccessedCompany.ContainsKey(companyId ))
            {
                user.LastAccessedCompany.Remove(companyId);
            }

            user.LastAccessedCompany.Add( companyId, DateTime.UtcNow );
            
            repository.Update(user);
        }

        //
        // GET: /Customer/Home/Edit

        public ActionResult Edit()
        {
            var model = QuestionnaireViewModel.CreateFrom(Service.GetCompany());
            if (Request.IsAjaxRequest())
            {
                return PartialView(model);
            }
            if (ControllerContext.IsChildAction)
            {
                return PartialView(model);
            }
            return View(model);
        }

        public ActionResult EditStore()
        {
            var company = Service.GetCompany();
            var viewModel = StoreSettingsViewModel
                .CreateFrom(company.Store, company.AppleAppStoreCredentials, company.GooglePlayCredentials);
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult EditStore(StoreSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Service.UpdateStoreSettings(Mapper.Map<StoreSettings>(model));
                    Service.UpdateAppleAppStoreCredentials(model.AppStoreCredentials);
                    Service.UpdateGooglePlayCredentials(model.GooglePlayCredentials);
                    return RedirectToAction("Index");
                }
                catch
                {
                    return View(model);
                }
            }
            return View(model);
        }


        [HttpPost]
        public ActionResult Edit(QuestionnaireViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Service.UpdateQuestionnaire(Mapper.Map<Questionnaire>(model));
                    return RedirectToAction("Index");
                }
                catch
                {
                    return View(model);
                }
            }
            return View(model);
        }

        public ActionResult EditAppDescription()
        {
            return View(Service.GetCompany());
        }

        [HttpPost]
        public ActionResult EditAppDescription([Bind(Include = "AppDescription")] Company model)
        {
            if (ModelState.IsValidField("AppDescription"))
            {
                try
                {
                    Service.UpdateAppDescription(model.AppDescription);
                    return RedirectToAction("Index");
                }
                catch
                {
                    return View(model);
                }
            }
            return View(model);
        }
    }
}