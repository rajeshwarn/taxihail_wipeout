#region

using System;
using System.Linq;
using System.Web.Mvc;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Attributes;
using CustomerPortal.Web.BitBucket;
using CustomerPortal.Web.Entities;
using MongoRepository;
using Environment = CustomerPortal.Web.Entities.Environment;

#endregion

namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    [NoCache]
    [Authorize(Roles = RoleName.Admin)]
    public class DeploymentController : DeployementControllerBase
    {
        private const string _allCompaniesKey = "allCompaniesKey";

        public DeploymentController(IRepository<DeploymentJob> repository)
            : base(repository)
        {
        }

        public DeploymentController()
            : this(new MongoRepository<DeploymentJob>())
        {
        }

        public ActionResult Index()
        {
            var model = new DeploymentJobModel();
            var companies = new MongoRepository<Company>();
            var companyList = companies.OrderBy(c => c.CompanyName).Select(c => new SelectListItem
            {
                Value = c.CompanyKey,
                Text = c.CompanyName + " (" + c.CompanyKey + ")"
            }).ToList();

            companyList.Add(new SelectListItem {Value = _allCompaniesKey, Text = "** All companies ** - USE CAREFULLY"});
            model.ModelForView.Company = companyList;

            var environments = new MongoRepository<Environment>();
            model.ModelForView.Environment = environments.OrderBy(e => e.Name).Select(e => new SelectListItem
            {
                Value = e.Id,
                Text = e.Name
            });

            var revisions = new MongoRepository<Revision>();
            model.ModelForView.Revision =
                revisions.Where(r => !r.Hidden && !r.Inactive ).OrderBy(r => r.Tag).Select(r => new SelectListItem
                {
                    Value = r.Id,
                    Text = r.Tag
                });

            model.Jobs = new MongoRepository<DeploymentJob>().OrderByDescending(d => d.Date).ToArray();
            return View(model);
        }



        public ActionResult Copy(string id)
        {
            var deploymentJob = Repository.Single(r => r.Id == id);
            if (deploymentJob.Type == DeploymentJobType.ServerPackage)
            {
                
                return RedirectToAction("CreateServerPackage", new { serverId = deploymentJob.Server.Id, revisionId = FindRevisionId(deploymentJob.Revision.Tag)  });                
            }
            else if (deploymentJob.Type == DeploymentJobType.DeployClient)
            {
                bool isProduction = deploymentJob.ServerUrl.Contains("services.taxihail.com");
                bool isStaging = deploymentJob.ServerUrl.Contains("staging.taxihail.com");                
                return RedirectToAction("DeployClient", new { companyId = deploymentJob.Company.CompanyKey, serverId = deploymentJob.Server.Id, revisionId = FindRevisionId(deploymentJob.Revision.Tag),
                                                                serverUrlIsProduction = isProduction, serverUrlIsStaging = isStaging, android = deploymentJob.Android, callbox = deploymentJob.CallBox, iosAdhoc = deploymentJob.IosAdhoc, iosAppStore = deploymentJob.IosAppStore});
            }
            if (deploymentJob.Type == DeploymentJobType.DeployServer)
            {
                return RedirectToAction("DeployServer", new { companyId = deploymentJob.Company.CompanyKey, serverId = deploymentJob.Server.Id, revisionId = FindRevisionId(deploymentJob.Revision.Tag) });
            }
            return null;
        }

        private string FindRevisionId(string tag)
        {
            var rev = new MongoRepository<Revision>().FirstOrDefault(r => r.Tag == tag);
            if (rev != null)
            {
                return rev.Id;
            }
            return null;
        }


        [NoCache]
        public ActionResult CreateServerPackage(string serverId = null, string revisionId = null)
        {
            var model = GetAddJobModel(e => e.Role == EnvironmentRole.BuildServer);

            model.Title = "Create Server Package";

            model.ServerId = serverId;
            model.RevisionId = revisionId;
            model.CreateType = (int) DeploymentJobType.ServerPackage;

            return View("Add", model);
        }

        
        public ActionResult DeployCustomer(string id)
        {
            var company = new MongoRepository<Company>().GetById(id);

            if (company == null) return HttpNotFound();


            var model = new DeployCustomerModel {Company = company, CompanyKey = company.CompanyKey};

            var revisions = new MongoRepository<Revision>();
            model.Revision =
                revisions.Where(r => r.CustomerVisible).OrderBy(r => r.Tag).Select(r => new SelectListItem
                {
                    Value = r.Id,
                    Text = r.Tag
                });

            return View(model);
        }

        [HttpPost]
        public ActionResult DeployCustomer(DeployCustomerModel model)
        {

            if (model.DeployOptions == DeployOptions.Both || model.DeployOptions == DeployOptions.MobileApp)
            {

                var job = new AddDeploymentJobModel { Android = true, CallBox = false, CompanyId = model.CompanyKey, CreateType = (int) DeploymentJobType.DeployClient , Database = false, IosAdhoc = true, IosAppStore = true, RevisionId = model.RevisionId , ServerUrlOptions = model.ServerUrlOptions  };                
                
                var environments = new MongoRepository<Environment>();
                job.ServerId = environments.Single(e => e.Name == "MobileBuildServer").Id;                
                AddDeploymentJob(job);                
            }

            if (model.DeployOptions == DeployOptions.Both || model.DeployOptions == DeployOptions.Server)
            {
               
                var rev = new MongoRepository<Revision>().GetById(model.RevisionId );
                
                var tagComponents = rev.Tag.Split( '.');
                bool useNewDeploymentService = (tagComponents.Length == 3) && (tagComponents[1] == "5") && (int.Parse(tagComponents[2]) >= 25);

                var job = new AddDeploymentJobModel { Android = false, CallBox = false, CompanyId = model.CompanyKey , CreateType = (int)DeploymentJobType.DeployServer, Database = true, IosAdhoc = false, IosAppStore = false, RevisionId = model.RevisionId, ServerUrlOptions = model.ServerUrlOptions };
                var environments = new MongoRepository<Environment>();
                job.ServerId = model.ServerUrlOptions == ServerUrlOptions.Production ? environments.Single(e =>  e.Name == (useNewDeploymentService ? "ProductionV2" : "Production")).Id : environments.Single(e => e.Name == "Staging").Id;
                AddDeploymentJob(job);                
            }




            return RedirectToAction("Index");
        }




        [HttpPost]
        public ActionResult CreateServerPackage(AddDeploymentJobModel model)
        {
            AddDeploymentJob(model);
            return RedirectToAction("Index");
        }

        [NoCache]
        public ActionResult DeployServer(string companyId = null,string serverId = null, string revisionId = null)
        {
            var model = GetAddJobModel(e => e.Role == EnvironmentRole.BuildServer || e.Role == EnvironmentRole.DeployServer);

            model.Title = "Deploy Server";
            model.ServerId = serverId;
            model.RevisionId = revisionId;
            model.CompanyId = companyId;
            model.CreateType = (int)DeploymentJobType.DeployServer;

            return View("Add", model);
        }

        [HttpPost]
        public ActionResult DeployServer(AddDeploymentJobModel model)
        {
            AddDeploymentJob(model);
            return RedirectToAction("Index");
        }
        [NoCache]
        public ActionResult DeployClient(string companyId = null, string serverId = null, string revisionId = null, bool serverUrlIsProduction = false, bool serverUrlIsStaging = false, bool android =false, bool callbox = false, bool iosAdhoc = false, bool iosAppStore = false)
        {
            var model = GetAddJobModel(e => e.Role == EnvironmentRole.BuildMobile);

            model.Title = "Deploy Client";
            model.CreateType = (int)DeploymentJobType.DeployClient;
            model.ServerId = serverId;
            model.RevisionId = revisionId;
            model.CompanyId = companyId;
            if ( serverUrlIsProduction || serverUrlIsStaging )
            {
                model.ServerUrlOptions = serverUrlIsStaging ? ServerUrlOptions.Staging : ServerUrlOptions.Production;
            }

            model.Android = android;
            model.CallBox = callbox;
            model.IosAdhoc = iosAdhoc;
            model.IosAppStore= iosAppStore;
            return View("Add", model);
        }

        [HttpPost]
        public ActionResult DeployClient(AddDeploymentJobModel model)
        {
            AddDeploymentJob(model);
            return RedirectToAction("Index");
        }
        public ActionResult UpdateVersions(int createType)
        {
            VersionUpdater.UpdateVersions();
            var type = (DeploymentJobType)createType;
            if (type == DeploymentJobType.ServerPackage)
            {
                return RedirectToAction("CreateServerPackage");
            }
            else if (type == DeploymentJobType.DeployClient)
            {
                return RedirectToAction("DeployClient");
            }
            else 
            {
                return RedirectToAction("DeployServer");
            }

            
        }
        private AddDeploymentJobModel GetAddJobModel(Func<Entities.Environment, bool> predicate)
        {
            var model = new AddDeploymentJobModel();

            var companies = new MongoRepository<Company>();
            var companyList = companies.OrderBy(c => c.CompanyName).Select(c => new SelectListItem
            {
                Value = c.CompanyKey,
                Text = c.CompanyName + " (" + c.CompanyKey + ")"
            }).ToList();

            companyList.Add(new SelectListItem {Value = _allCompaniesKey, Text = "** All companies ** - USE CAREFULLY"});
            model.ModelForView.Company = companyList;

            var environments = new MongoRepository<Environment>();

            model.ModelForView.Environment =
                environments.ToArray().Where(predicate).OrderBy(e => e.Name).Select(e => new SelectListItem
                {
                    Value = e.Id,
                    Text = e.Name
                });

            var revisions = new MongoRepository<Revision>();
            model.ModelForView.Revision = revisions.Where(r => !r.Hidden && !r.Inactive).OrderBy(r => r.Tag).Select(r => new SelectListItem
            {
                Value = r.Id,
                Text = string.Format( "{0} ({1})", r.Tag , r.Commit )
            });
            model.Database = true;
            return model;
        }


        public ActionResult Clear()
        {
            Repository.Delete(
                r => (r.Status != JobStatus.Inprogress.ToString()) && (r.Status != JobStatus.Requested.ToString()));

            return RedirectToAction("Index");
        }

        public ActionResult ForceClear()
        {
            Repository.DeleteAll();

            return RedirectToAction("Index");
        }

        public ActionResult Delete(string id)
        {

            Repository.Delete(r => r.Id == id);

            return RedirectToAction("Index");
        }

        public ActionResult Details(string id)
        {
            var deploymentJob = Repository.Single(r => r.Id == id);

            return View(deploymentJob);
        }

        [NoCache]
        public ActionResult DetailsText(string id)
        {
            var deploymentJob = Repository.Single(r => r.Id == id);

            return Content(deploymentJob.Details);
        }

        [NoCache]
        public ActionResult StatusText(string id)
        {
            var deploymentJob = Repository.Single(r => r.Id == id);

            return Content(deploymentJob.Status);
        }


        private void AddDeploymentJob(AddDeploymentJobModel model, string email = null)
        {
            Action<Company> addACompanyAction = company =>
            {
                var deploy = new DeploymentJob();
                deploy.Id = Guid.NewGuid().ToString();
                deploy.Date = DateTime.Now;
                deploy.Company = company;

                var environments = new MongoRepository<Environment>();
                deploy.Server = environments.Single(e => e.Id == model.ServerId);
                var revisions = new MongoRepository<Revision>();
                deploy.Revision = revisions.Single(r => r.Id == model.RevisionId);

                var createType = (DeploymentJobType) model.CreateType;

                deploy.ServerSide = createType == DeploymentJobType.ServerPackage ||
                                    createType == DeploymentJobType.DeployServer;
                deploy.ClientSide = !deploy.ServerSide;
                deploy.Type = createType;

                if (deploy.ServerSide && (createType == DeploymentJobType.ServerPackage))
                {
                    deploy.ServerSide = false;
                    deploy.Database = false;
                }
                else if (deploy.ServerSide)
                {
                    deploy.Database = model.Database;
                }
                else
                {
                    deploy.ServerUrl = GetUrlFromModel(model, company);
                    if (string.IsNullOrWhiteSpace(deploy.ServerUrl))
                    {
                        throw new InvalidOperationException("Server url must be set");
                    }
                    deploy.Android = model.Android;
                    deploy.CallBox = model.CallBox;
                    deploy.IosAdhoc = model.IosAdhoc;
                    deploy.IosAppStore = (model.ServerUrlOptions == ServerUrlOptions.Production) && model.IosAppStore;
                }


                company.LastKnownProductionVersion = null;
                company.LastKnownStagingVersion = null;

                new MongoRepository<Company>().Update(company);

                Repository.Add(deploy);
            };


            var companies = new MongoRepository<Company>();

            if (model.CompanyId == _allCompaniesKey)
            {
                foreach (var company in companies.Where(c => (c.Status == AppStatus.Production) || (c.Status == AppStatus.TestingNewVersion)))
                {
                    addACompanyAction(company);
                }
            }
            else if (model.CreateType == (int) DeploymentJobType.ServerPackage)
            {
                var company = companies.Single(c => c.CompanyKey == "TaxiHailDemo");
                addACompanyAction(company);
            }
            else
            {
                var company = companies.Single(c => c.CompanyKey == model.CompanyId);
                addACompanyAction(company);
            }
        }

        private string GetUrlFromModel(AddDeploymentJobModel model, Company company)
        {
            if (model.ServerUrlOptions == ServerUrlOptions.Other)
            {
                return model.ServerUrl;
            }
            if (model.ServerUrlOptions == ServerUrlOptions.Production)
            {
                return string.Format("https://services.taxihail.com/{0}/api/", company.CompanyKey);
            }
            if (model.ServerUrlOptions == ServerUrlOptions.Staging)
            {
                return string.Format("http://staging.taxihail.com/{0}/api/", company.CompanyKey);
            }
            if (model.ServerUrlOptions == ServerUrlOptions.Dev)
            {
                return string.Format("http://test.taxihail.biz:8181/{0}/api/", company.CompanyKey);
            }
            return null;
        }
    }
}