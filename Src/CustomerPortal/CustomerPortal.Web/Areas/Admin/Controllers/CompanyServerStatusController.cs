using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Attributes;
using CustomerPortal.Web.Entities;
using MongoRepository;

namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    public class CompanyServerStatusController : Controller
    {
        private readonly IRepository<CompanyServerStatus> _companyServerStatusRepository;


        public CompanyServerStatusController() : this(new MongoRepository<CompanyServerStatus>())
        {

        }


        public CompanyServerStatusController(IRepository<CompanyServerStatus> companyServerStatusRepository)
        {
            _companyServerStatusRepository = companyServerStatusRepository;
        }

        [NoCache]
        public ActionResult StatusDetails(string companyKey)
        {
            var status = _companyServerStatusRepository
                .FirstOrDefault(companyStatus => companyStatus.CompanyKey == companyKey);

            return View("_StatusDetails", GetModelFromDbObject(status));
        }

        [NoCache]
        public ActionResult Details(string companyKey)
        {
            var status = _companyServerStatusRepository
                .FirstOrDefault(companyStatus => companyStatus.CompanyKey == companyKey);

            return View(GetModelFromDbObject(status));
        }


        private CompanyStatusModel GetModelFromDbObject(CompanyServerStatus status)
        {
            return new CompanyStatusModel
            {
                CompanyKey = status.CompanyKey,
                CompanyName = status.CompanyName,
                ServiceStatus = status.ServiceStatus,
                IsServerAvailable = status.IsServerAvailable,
                IsApiAvailable = status.IsApiAvailable,
                HasAuthenticationError = status.HasAuthenticationError
            };
        }

        [NoCache]
        public ActionResult StatusList()
        {
            var companyStatus = GetCompanyStatus();

            return View("_StatusList", companyStatus);
        }

        [NoCache]
        public ActionResult Index()
        {
            var companyStatus = GetCompanyStatus();

            return View(companyStatus);
        }

        private CompanyStatusModel[] GetCompanyStatus()
        {
            return _companyServerStatusRepository
                .Select(GetModelFromDbObject)
                .OrderBy(model => model, new ServerStatusComparer())
                .ThenBy(model => model.CompanyName)
                .ToArray();
        }


        private class ServerStatusComparer : IComparer<CompanyStatusModel>
        {
            public int Compare(CompanyStatusModel x, CompanyStatusModel y)
            {
                var weightX = GetWeight(x.ServiceState);
                var weightY = GetWeight(y.ServiceState);

                return weightX - weightY;
            }


            private int GetWeight(ServiceStatusType type)
            {
                if (type == ServiceStatusType.IssuesFound)
                {
                    return 0;
                }

                if (type == ServiceStatusType.NoConnection)
                {
                    return 1;
                }

                return type == ServiceStatusType.Healthy
                    ? 2
                    : 3;
            }

        }

    }
}