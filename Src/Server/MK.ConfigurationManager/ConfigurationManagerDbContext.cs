using System.Data.Entity;
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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Company>().ToTable("Company", SchemaName);
            modelBuilder.Entity<IBSServer>().ToTable("IBSServer", SchemaName);
            modelBuilder.Entity<TaxiHailEnvironment>().ToTable("TaxiHailEnvironment", SchemaName);
        }
    }
}
