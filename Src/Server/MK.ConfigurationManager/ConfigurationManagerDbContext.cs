using System.Data.Entity;
using System.Linq;
using MK.ConfigurationManager.Entities;

namespace MK.ConfigurationManager
{
    class ConfigurationManagerDbContext : DbContext 
    {
        public const string SchemaName = "MkConfig";

        public ConfigurationManagerDbContext()
        {
            
        }

        public ConfigurationManagerDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public IQueryable<DeploymentJob> DeploymentJobs
        {
            get
            {
                var x=  Set<DeploymentJob>()
                    .Include(j => j.Company)
                    .Include(j => j.Version)
                    .Include(j => j.IBSServer)
                    .Include(j => j.TaxHailEnv);
                return x;
            } 
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Company>().ToTable("Company", SchemaName);
            modelBuilder.Entity<IBSServer>().ToTable("IBSServer", SchemaName);
            modelBuilder.Entity<TaxiHailEnvironment>().ToTable("TaxiHailEnvironment", SchemaName);
            modelBuilder.Entity<AppVersion>().ToTable("AppVersion", SchemaName);
            modelBuilder.Entity<DeploymentJob>().ToTable("DeploymentJob", SchemaName);
        }
    }
}
