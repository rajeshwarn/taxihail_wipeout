#region

using System.Linq;
using System.Web.Http;
using CustomerPortal.Web.Entities;
using MongoRepository;

#endregion

namespace CustomerPortal.Web.Controllers
{
    [Authorize(Roles = RoleName.Admin)]
    public class DeploymentJobController : ApiController
    {
        private readonly IRepository<DeploymentJob> _repository;
        private readonly IRepository<DefaultCompanySetting> _repositoryDefaultSettings;

        public DeploymentJobController(IRepository<DeploymentJob> repository, IRepository<DefaultCompanySetting> repositoryDefaultSettings)
        {
            _repository = repository;
            _repositoryDefaultSettings = repositoryDefaultSettings;
        }

        public DeploymentJobController()
            : this(new MongoRepository<DeploymentJob>(), new MongoRepository<DefaultCompanySetting>())
        {
        }

        // GET api/deploymentjob
        public DeploymentJob Get([FromUri] bool isServerSide)
        {
            var deploymentJob = (from job in _repository
                where job.ServerSide == isServerSide
                orderby job.Date
                select job).FirstOrDefault();

            ApplyDefaults(deploymentJob.Company);
            return deploymentJob;
        }

        private Company ApplyDefaults(Company company)
        {
            foreach (var defaultSetting in _repositoryDefaultSettings)
            {
                if (company.CompanySettings.All(c => c.Key != defaultSetting.Id))
                {
                    company.CompanySettings.Add(new CompanySetting
                    {
                        Key = defaultSetting.Id,
                        Value = defaultSetting.Value,
                        IsClientSetting = defaultSetting.IsClient
                    });
                }
            }
            return company;

        }
        
        // PUT api/deploymentjob/5
        public void Put(int id, [FromBody]DeploymentJob value)
        {
        }
    }
}