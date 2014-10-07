#region

using System.Linq;
using System.Web.Http;
using CustomerPortal.Web.Entities;
using MongoRepository;

#endregion

namespace CustomerPortal.Web.Controllers.API
{
    [Authorize(Roles = RoleName.Admin)]
    public class DeploymentController : ApiController
    {
        private readonly IRepository<DeploymentJob> _repository;
            private readonly IRepository<DefaultCompanySetting> _repositoryDefaultSettings;

            public DeploymentController(IRepository<DeploymentJob> repository, IRepository<DefaultCompanySetting> repositoryDefaultSettings)
        {
            _repository = repository;
            _repositoryDefaultSettings = repositoryDefaultSettings;
        }

            public DeploymentController()
                : this(new MongoRepository<DeploymentJob>(), new MongoRepository<DefaultCompanySetting>())
        {
        }


        [Route("api/deployments/{serverName}/next")]
        public DeploymentJob GetNextForServer(string serverName)
        {
            var deployment =
                _repository.FirstOrDefault(
                    d => d.Server.Name == serverName && d.Status == JobStatus.Requested.ToString());

            if (deployment != null)
            {
                ApplyDefaults(deployment.Company);
            }

            return deployment;
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

        [Route("api/deployments/{id}/updatedetails")]
        public void UdpateDetails(string id, JobStatusDetails details)
        {
            var deployment = _repository.SingleOrDefault(d => d.Id == id);
            if (deployment != null)
            {
                if (string.IsNullOrWhiteSpace(deployment.Details))
                {
                    deployment.Details += details.Details;
                }
                else
                {
                    deployment.Details += "<br>" + details.Details;
                }

                if (details.Status.HasValue)
                {
                    deployment.Status = details.Status.Value.ToString();
                }

                _repository.Update(deployment);
            }
        }
    }
}