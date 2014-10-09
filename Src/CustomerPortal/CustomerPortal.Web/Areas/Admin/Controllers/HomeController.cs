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
using CustomerPortal.Web.Services.Impl;

#endregion

namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleName.Admin)]
    public class HomeController : CompanyControllerBase
    {
        public enum ViewType
        {
            Default,
            Demo,
            All,
            OrderByPublishDate,
            NotPublished,
            AssetReady,
            Open,
            Last10,
            StoreAvailableInTest,
        }

        public static bool Is15Ready(Company company)
        {
            var logo = new AssetsManager(company.Id).GetAll().FirstOrDefault(f => Path.GetFileName(f).ToLower() == "logo_1_5.png");
            return logo != null;
        }

        private readonly MongoSession _session =
            new MongoSession(ConfigurationManager.ConnectionStrings["MongoServerSettings"].ConnectionString);

        public HomeController(IRepository<Company> repository)
            : base(repository)
        {
        }

        public HomeController()
            : this(new MongoRepository<Company>())
        {
        }

        public ActionResult ForceVersionRefresh(string id)
        {
            var repository = new MongoRepository<Company>();
            var company = repository.GetById(id);


            company.LastKnownStagingVersion = GetVersion("staging", company.CompanyKey);
            company.LastKnownProductionVersion = GetVersion("services", company.CompanyKey);
            repository.Update(company);

            return RedirectToAction("Index");
        }

        public ActionResult RefreshVersions()
        {
            Task.Factory.StartNew(() =>
            {
                var repository = new MongoRepository<Company>();
                foreach (var company in repository)
                {
                    if (string.IsNullOrEmpty(company.LastKnownStagingVersion) ||
                        string.IsNullOrEmpty(company.LastKnownProductionVersion))
                    {
                        if (string.IsNullOrEmpty(company.LastKnownStagingVersion))
                        {
                            company.LastKnownStagingVersion = GetVersion("staging", company.CompanyKey);
                        }

                        if (string.IsNullOrEmpty(company.LastKnownProductionVersion))
                        {
                            company.LastKnownProductionVersion = GetVersion("services", company.CompanyKey);
                        }

                        new MongoRepository<Company>().Update(company);


                    }
                }
            });

            return RedirectToAction("Index");
        }





        private string GetVersion(string server, string companyKey)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var url = string.Format("http://{0}.taxihail.com/{1}/api/app/info?format=json", server, companyKey);

                    var appInfo =
                        client.GetAsync(url)
                            .Result.Content.ReadAsAsync<ApplicationInfo>()
                            .Result;
                    return appInfo.Version;
                }
            }
            catch
            {
                return null;
            }
        }

        //
        // GET: /Customer/Home/

        public ActionResult Index()
        {

            return RedirectToAction(GetLastViewAccessByUser().ToString());

        }

        private ViewType GetLastViewAccessByUser()
        {
            var repository = new MongoRepository<UserPreference>();
            var user = repository.SingleOrDefault(u => u.UserIdentity == User.Identity.Name);
            if (user == null)
            {
                return ViewType.Default;
            }
            else
            {
                return (ViewType)Enum.Parse(typeof(ViewType), user.HomeViewSelected);
            }

        }


      
        private IEnumerable<string> GetLast10AccessedCompanies()
        {
            var repository = new MongoRepository<UserPreference>();
            var user = repository.SingleOrDefault(u => u.UserIdentity == User.Identity.Name);
            if (user == null)
            {
                return new string[0];
            }
            else
            {
                return user.LastAccessedCompany.OrderByDescending(l => l.Value).Take(10).Select(l => l.Key);
            }

        }

        private void SetLastViewAccessByUser(ViewType viewType)
        {
            var repository = new MongoRepository<UserPreference>();
            var user = repository.SingleOrDefault(u => u.UserIdentity == User.Identity.Name);
            if (user == null)
            {
                user = new UserPreference();
                user.Id = Guid.NewGuid().ToString();
                user.UserIdentity = User.Identity.Name;
            }

            user.HomeViewSelected = viewType.ToString();
            repository.Update(user);
        }

        public ActionResult Default()
        {
            SetLastViewAccessByUser(ViewType.Default);

            ViewBag.ViewType = ViewType.Default.ToString();

            var repository = new MongoRepository<Company>();

            return View("Index", repository.Where(x => x.Status != AppStatus.DemoSystem).OrderBy(x => x.CompanyName).ToArray());
        }

        public ActionResult Demo()
        {
            SetLastViewAccessByUser(ViewType.Demo);

            ViewBag.ViewType = ViewType.Demo.ToString();

            var repository = new MongoRepository<Company>();

            return View("Index", repository.Where(x => x.Status == AppStatus.DemoSystem).OrderBy(x => x.CompanyName).ToArray());
        }

        public ActionResult All()
        {
            SetLastViewAccessByUser(ViewType.All);

            ViewBag.ViewType = ViewType.All.ToString();

            var repository = new MongoRepository<Company>();

            return View("Index", repository.OrderBy(x => x.CompanyName).ToArray());
        }
        public ActionResult OrderByPublishDate()
        {
            SetLastViewAccessByUser(ViewType.OrderByPublishDate);

            ViewBag.ViewType = ViewType.OrderByPublishDate.ToString();

            var repository = new MongoRepository<Company>();

            return View("Index", repository.ToArray().Where(c => c.Store.PublishedDate.HasValue).OrderByDescending(x => x.Store.PublishedDate.Value).ToArray());
        }

        public ActionResult NotPublished()
        {

            SetLastViewAccessByUser(ViewType.NotPublished);

            ViewBag.ViewType = ViewType.NotPublished.ToString();

            var repository = new MongoRepository<Company>();

            return View("Index", repository.ToArray().Where(c => c.Status != AppStatus.DemoSystem && !c.Store.PublishedDate.HasValue).OrderBy(x => x.CompanyName).ToArray());
        }

        public ActionResult AssetReady()
        {
            SetLastViewAccessByUser(ViewType.AssetReady);

            ViewBag.ViewType = ViewType.Open.ToString();

            var repository = new MongoRepository<Company>();

            return View("Index", repository.ToArray().Where(c => c.Status == AppStatus.AssetReady).OrderBy(x => x.CompanyName).ToArray());
        }
        public ActionResult Open()
        {
            SetLastViewAccessByUser(ViewType.Open);

            ViewBag.ViewType = ViewType.Open.ToString();

            var repository = new MongoRepository<Company>();

            return View("Index", repository.ToArray().Where(c => c.Status == AppStatus.Open).OrderBy(x => x.CompanyName).ToArray());
        }

        public ActionResult StoreAvailableInTest()
        {
            SetLastViewAccessByUser(ViewType.StoreAvailableInTest);

            ViewBag.ViewType = ViewType.StoreAvailableInTest.ToString();

            var repository = new MongoRepository<Company>();

            return View("Index", repository.ToArray().Where(c => (c.Status == AppStatus.Open || c.Status == AppStatus.AssetReady || c.Status == AppStatus.Test) && !string.IsNullOrWhiteSpace(c.Store.AndroidStoreUrl)).OrderBy(x => x.CompanyName).ToArray());
        }

        

        public ActionResult Last10()
        {
            SetLastViewAccessByUser(ViewType.Last10);

            ViewBag.ViewType = ViewType.Last10.ToString();

            var repository = new MongoRepository<Company>();

            return View("Index", GetLast10AccessedCompanies().Select(c => repository.GetById(c)).Where(c => c != null).ToArray());
        }





        //
        // GET: /Admin/Company/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Admin/Company/Create

        [HttpPost]
        public ActionResult Create(Company model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    try
                    {
                        model.Id = model.CompanyKey = new CompanyIdGenerator().Generate(model.CompanyName);
                    }
                    catch (InvalidOperationException e)
                    {
                        ModelState.AddModelError("", "Error generating Company Id");
                        return View(model);
                    }
                    string path = @"~/Areas/Admin/SettingsDataSrc/Common.json";
                    if (!System.IO.File.Exists(path))
                    {
                        var json =
                            System.IO.File.ReadAllText(
                                HostingEnvironment.MapPath(path));
                        var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                        foreach (KeyValuePair<string, string> setting in settings)
                        {
                            model.CompanySettings.Add(new CompanySetting { Key = setting.Key, Value = setting.Value });
                        }
                    }
                    else
                    {
                        throw new FileNotFoundException();
                    }
                    Repository.Add(model);
                    return RedirectToAction("Create", "Membership", new { id = model.Id, cw = "1" });
                }
                catch
                {
                    return View(model);
                }
            }
            return View(model);
        }

        //
        // GET: /Admin/Company/Edit/5
        [ChildActionOnly]
        public ActionResult Edit(string id)
        {
            var company = Repository.GetById(id);
            if (company == null) return HttpNotFound();

            return PartialView(company);
        }

        //
        // POST: /Admin/Company/Edit/5

        [HttpPost]
        public ActionResult Edit(string id, Company model)
        {
            var company = Repository.GetById(id);

            if (company == null) return HttpNotFound();

            try
            {



                if (TryUpdateModel(company,
                    new[]
                    {
                        "CompanyName",
                        "ProjectContactName",
                        "ProjectContactEmail",
                        "ProjectContactTel"
                    }))
                {
                    Repository.Update(company);
                }
                return PartialView(company);
            }
            catch
            {
                return PartialView(company);
            }
        }


        //
        // GET: /Admin/Company/Delete/5

        public ActionResult Delete(string id)
        {
            var company = Repository.GetById(id);
            if (company == null) return HttpNotFound();

            return View(company);
        }

        //
        // POST: /Admin/Company/Delete/5

        [HttpPost]
        public ActionResult Delete(string id, FormCollection ignoreThis)
        {
            try
            {
                Repository.Delete(id);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Manage(string id)
        {
            var repository = new MongoRepository<Company>();
            var company = repository.GetById(id);
            if (company == null)
            {
                company = Repository.FirstOrDefault(c => c.CompanyKey == id);
            }

            if (company == null) return HttpNotFound();

            Session[SessionKey.AdminCurrentCompany] = company.Id;

            return RedirectToAction("Index", "Home", new { area = "customer" });
        }

        public ActionResult Status(string id)
        {
            var company = Repository.GetById(id);
            if (company == null) return HttpNotFound();

            return View(company);
        }

        public ActionResult CreateKiosk(string id)
        {
            var company = Repository.GetById(id);
            if (company == null) return HttpNotFound();

            if (Repository.Any(c => c.CompanyKey == company.CompanyKey + "Kiosk"))
            {
                return Manage(Repository.First(c => c.CompanyKey == company.CompanyKey + "Kiosk").Id);
            }

            company.Id = company.CompanyKey += "Kiosk";
            company.CompanyKey = company.Id;
            company.CompanyName = company.Id;


            AppendToSettingValue(company, "TaxiHail.ApplicationKey", "Kiosk");
            AppendToSettingValue(company, "TaxiHail.ApplicationName", " Kiosk");
            AppendToSettingValue(company, "TaxiHail.SiteName", " Kiosk");
            AppendToSettingValue(company, "ApplicationName", " Kiosk");
            AppendToSettingValue(company, "AppName", " Kiosk");

            Repository.Update(company);

            new AssetsManager(id).CopyTo(company.Id, "public.keystore");
            new WebThemeFilesManager(id).CopyTo(company.Id);



            return Manage(company.Id);
        }

        private void AppendToSettingValue(Company company, string key, string toAppend)
        {
            var setting = company.CompanySettings.FirstOrDefault(c => c.Key == key);
            if (setting != null)
            {
                setting.Value += toAppend;
            }
        }

        [HttpPost]
        public ActionResult Status(string id, AppStatus status)
        {
            var company = Repository.GetById(id);
            if (company == null) return HttpNotFound();
            var oldStat = company.Status;
            var changeDate = DateTime.Now;
            company.Status = status;
            Repository.Update(company);

            ChangeStatusEmail.SendEmail(oldStat.ToString(), company.Status.ToString(), _session, changeDate,
                company.CompanyName, company.Payment.PONumber);
            return RedirectToAction("Index");
        }
    }
}