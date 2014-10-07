#region

using CustomerPortal.Web.Entities;

#endregion

namespace CustomerPortal.Web.Areas.Admin.Models
{
    public class DeploymentJobModel
    {
        public DeploymentJobModel()
        {
            Jobs = new DeploymentJob[0];
            ModelForView = new DeploymentModel();
            DomainModel = new DeploymentJob();
        }

        public DeploymentJob[] Jobs { get; set; }
        public DeploymentModel ModelForView { get; set; }
        public DeploymentJob DomainModel { get; set; }
    }
}