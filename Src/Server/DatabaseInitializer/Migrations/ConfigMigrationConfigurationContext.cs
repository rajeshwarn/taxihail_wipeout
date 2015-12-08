namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class ConfigMigrationConfigurationContext : DbMigrationsConfiguration<apcurium.MK.Common.Configuration.Impl.ConfigurationDbContext>
    {
        public ConfigMigrationConfigurationContext()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(apcurium.MK.Common.Configuration.Impl.ConfigurationDbContext context)
        {
            
        }
    }
}
